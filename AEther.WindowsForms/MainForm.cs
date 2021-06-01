using System;
using System.Collections.Generic;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Concurrent;
using System.Linq;

using AEther.DMX;
using AEther.CSCore;

using SharpDX.Direct3D11;
using System.Threading.Tasks.Dataflow;
using System.DirectoryServices;

namespace AEther.WindowsForms
{
    public partial class MainForm : Form
    {

        const string LoopbackEntry = "Loopback";
        const bool UseMapping = true;
        const bool UseFloatTextures = false;

        bool IsRunning = false;
        bool IsRendering = false;
        CancellationTokenSource Cancel = new();
        SpectrumAccumulator[] Spectrum = Array.Empty<SpectrumAccumulator>();
        Histogram[] Histogram = Array.Empty<Histogram>();

        TimeSpan Latency;
        TimeSpan FrameTime;
        int EventCounter = 0;

        readonly Stopwatch FrameTimer = Stopwatch.StartNew();
        readonly Stopwatch LatencyUpdateTimer = Stopwatch.StartNew();
        readonly TimeSpan LatencyUpdateInterval = TimeSpan.FromSeconds(1);

        readonly Graphics Graphics;
        readonly Shader HistogramShader;
        readonly Shader SpectrumShader;
        readonly EffectScalarVariable HistogramShiftVariable;
        readonly TaskScheduler UIScheduler;

        public MainForm()
        {

            UIScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            InitializeComponent();
            Graphics = new Graphics(Handle);
            HistogramShader = Graphics.CreateShader("histogram.fx");
            SpectrumShader = Graphics.CreateShader("spectrum.fx");

            HistogramShiftVariable = HistogramShader.Variables["HistogramShift"].AsScalar();

            var audioDevices = Recorder.GetAvailableDeviceNames();
            Input.Items.Clear();
            Input.Items.Add(LoopbackEntry);
            Input.Items.AddRange(audioDevices.ToArray());
            Input.SelectedItem = LoopbackEntry;

            State.Items.Clear();
            State.Items.AddRange(new GraphicsState[]
            {
                new ShaderState(Graphics, HistogramShader, "Histogramm"),
                new ShaderState(Graphics, SpectrumShader, "Spectrum"),
                new FluidState(Graphics),
                new IFSState(Graphics),
            });
            State.SelectedIndex = 0;

            Options.SelectedObject = new SessionOptions();

        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            Graphics.Resize(ClientSize.Width, ClientSize.Height);
        }

        protected async override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            await StopAsync();
            Graphics.Dispose();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Graphics.Resize(ClientSize.Width, ClientSize.Height);

            _ = Task.Factory.StartNew(() =>
            {
                while (!IsDisposed)
                {
                    IsRendering = true;
                    Render();
                    IsRendering = false;
                    Application.DoEvents();
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, UIScheduler);

            _ = Task.Run(RunAsync);


            //SharpDX.Windows.RenderLoop.Run(this, Render);

        }

        protected async override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    TogglePanel();
                    break;
                case Keys.F5:
                    await StopAsync();
                    _ = Task.Run(RunAsync);
                    break;
                case Keys.F11:
                    ToggleFullscreen();
                    break;
                case Keys.Escape:
                    Close();
                    break;
                default:
                    base.OnKeyDown(e);
                    break;
            }
        }

        void TogglePanel()
        {
            tlpPanel.Visible ^= true;
        }


        async Task RunAsync()
        {

            Cancel = new();

            (AudioDevice, SessionOptions) Init()
            {
                var deviceName = Input.SelectedItem.ToString();
                AudioDevice device = deviceName is LoopbackEntry
                    ? new Loopback()
                    : new Recorder(deviceName ?? string.Empty);
                if (Options.SelectedObject is not SessionOptions options)
                {
                    throw new InvalidCastException();
                }
                Spectrum = CreateSpectrum(device.Format.ChannelCount, options.Domain.Count);
                Histogram = CreateHistogram(device.Format.ChannelCount, options.Domain.Count, options.TimeResolution);


                return (device, options);
            }

            var (device, options) = await Task.Factory.StartNew(Init, Cancel.Token, TaskCreationOptions.None, UIScheduler);
            var sampleSource = new AudioInput(device);
            var session = new Session(sampleSource, options);

            DMXController? dmx = null;
            if (0 < options.DMXPort)
            {
                var comPort = $"COM{options.DMXPort}";
                dmx = new DMXController(comPort, options.Domain)
                {
                    SinuoidThreshold = options.SinuoidThreshold,
                    TransientThreshold = options.TransientThreshold,
                };
            }

            Cancel.Token.Register(sampleSource.Stop);
            var events = session.RunAsync();

            IsRunning = true;
            sampleSource.Start();
            try
            {
                await foreach (var evt in events)
                {

                    var latency = DateTime.Now - evt.Time;
                    if (Latency < latency)
                    {
                        Latency = latency;
                    }

                    dmx?.Process(evt);

                    for (int c = 0; c < sampleSource.Format.ChannelCount; ++c)
                    {
                        var channel = evt.GetChannel(c);
                        Spectrum[c].Add(channel.Span);
                        Histogram[c].Add(channel.Span);
                    }

                    evt.Dispose();
                    Interlocked.Increment(ref EventCounter);
                }
            }
            catch (Exception) { }

            if (dmx is not null)
            {
                await dmx.DisposeAsync();
            }
            sampleSource.Dispose();
            IsRunning = false;

        }

        void ToggleFullscreen()
        {
            if (Graphics.IsFullscreen)
            {
                Hide();
                Show();
            }
            else
            {
                Graphics.Resize(Graphics.NativeMode);
                Graphics.IsFullscreen = true;
            }
        }

        public void Render()
        {

            if (IsDisposed)
                return;

            if (!IsRunning)
                return;

            var frameTime = FrameTimer.Elapsed;
            FrameTimer.Restart();

            if (FrameTime < frameTime)
            {
                FrameTime = frameTime;
            }

            if (LatencyUpdateInterval < LatencyUpdateTimer.Elapsed)
            {
                Text = $"Latency: {Math.Round(Latency.TotalMilliseconds, 0)} ms, Draw Time: {Math.Round(FrameTime.TotalMilliseconds, 0)} ms";
                Latency = TimeSpan.Zero;
                FrameTime = TimeSpan.Zero;
                LatencyUpdateTimer.Restart();
            }

            if (0 < Interlocked.Exchange(ref EventCounter, 0))
            {

                foreach (var spectrum in Spectrum)
                {
                    spectrum.Update();
                    spectrum.Clear();
                }
                SpectrumShader.ShaderResources["Spectrum0"].SetResource(Spectrum[0].Texture.GetShaderResourceView());
                SpectrumShader.ShaderResources["Spectrum1"].SetResource(Spectrum[1].Texture.GetShaderResourceView());

                foreach (var histogram in Histogram)
                {
                    histogram.Update();
                }
                HistogramShader.ShaderResources["Histogram0"].SetResource(Histogram[0].Texture.GetShaderResourceView());
                HistogramShader.ShaderResources["Histogram1"].SetResource(Histogram[1].Texture.GetShaderResourceView());

                var histogramShift = (Histogram[0].Position - .1f) / Histogram[0].Texture.Height;
                HistogramShiftVariable.Set(histogramShift);

            }

            if (State.SelectedItem is GraphicsState state)
            {
                using var frame = Graphics.RenderFrame();
                state.Render();
            }

        }

        SpectrumAccumulator[] CreateSpectrum(int channelCount, int noteCount)
        {
            var spectrum = new SpectrumAccumulator[channelCount];
            for (var c = 0; c < spectrum.Length; ++c)
            {
                spectrum[c] = UseFloatTextures
                    ? new FloatSpectrum(Graphics, noteCount)
                    : new ByteSpectrum(Graphics, noteCount);
            }
            return spectrum;
        }

        Histogram[] CreateHistogram(int channelCount, int noteCount, int historyCount)
        {
            var histogram = new Histogram[channelCount];
            for (var c = 0; c < histogram.Length; ++c)
            {
                histogram[c] = UseFloatTextures
                    ? new FloatHistogram(Graphics, noteCount, historyCount, UseMapping)
                    : new ByteHistogram(Graphics, noteCount, historyCount, UseMapping);
            }
            return histogram;
        }

        async Task StopAsync()
        {
            Cancel.Cancel();
            while (IsRunning)
            {
                await Task.Delay(10);
            }
            while (IsRendering)
            {
                await Task.Delay(10);
            }
            foreach (var spectrum in Spectrum)
            {
                spectrum.Dispose();
            }
            foreach (var histogram in Histogram)
            {
                histogram.Dispose();
            }
        }

        private async void Options_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (IsRunning)
            {
                await StopAsync();
                _ = Task.Run(RunAsync);
            }
        }

        private async void Input_SelectedValueChanged(object sender, EventArgs e)
        {
            if (IsRunning)
            {
                await StopAsync();
                _ = Task.Run(RunAsync);
            }
        }
    }
}