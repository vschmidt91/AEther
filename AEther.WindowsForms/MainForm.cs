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
using System.Reflection;

namespace AEther.WindowsForms
{
    public partial class MainForm : Form
    {

        const string LoopbackEntry = "Loopback";
        const bool UseMapping = true;
        const bool UseFloatTextures = false;

        SpectrumAccumulator[] Spectrum = Array.Empty<SpectrumAccumulator>();
        Histogram[] Histogram = Array.Empty<Histogram>();

        CancellationTokenSource Cancel = new CancellationTokenSource();
        Session? Session = null;
        Task SessionTask = Task.CompletedTask;
        TimeSpan Latency = TimeSpan.Zero;
        TimeSpan FrameTime = TimeSpan.Zero;
        int EventCounter = 0;

        readonly Stopwatch FrameTimer = Stopwatch.StartNew();
        readonly Stopwatch LatencyUpdateTimer = Stopwatch.StartNew();
        readonly TimeSpan LatencyUpdateInterval = TimeSpan.FromSeconds(1);
        readonly TaskScheduler UIScheduler;

        Graphics Graphics;
        Shader HistogramShader;
        Shader SpectrumShader;
        Shader MandelboxShader;
        EffectScalarVariable HistogramShiftVariable;
        EffectScalarVariable HistogramShiftVariable2;

        public MainForm()
        {

            InitializeComponent();
            UIScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            InitGraphics();

            var audioDevices = Recorder.GetAvailableDeviceNames();
            Input.Items.Clear();
            Input.Items.Add(LoopbackEntry);
            Input.Items.AddRange(audioDevices.ToArray());
            Input.SelectedItem = LoopbackEntry;

            Options.SelectedObject = new SessionOptions();

        }

        void InitGraphics()
        {

            Graphics = new Graphics(Handle);
            HistogramShader = Graphics.CreateShader("histogram.fx");
            SpectrumShader = Graphics.CreateShader("spectrum.fx");
            MandelboxShader = Graphics.CreateShader("mandelbox.fx");

            HistogramShiftVariable = HistogramShader.Variables["HistogramShift"].AsScalar();
            HistogramShiftVariable2 = MandelboxShader.Variables["HistogramShift"].AsScalar();

            State.Items.Clear();
            State.Items.AddRange(new GraphicsState[]
            {
                new ShaderState(Graphics, HistogramShader, "Histogramm"),
                new ShaderState(Graphics, SpectrumShader, "Spectrum"),
                new ShaderState(Graphics, MandelboxShader, "Mandelbox"),
                new FluidState(Graphics),
                new IFSState(Graphics),
            });
            State.SelectedIndex = 0;
            Graphics.Shaders.FileChanged += Shaders_FileChanged;

        }

        private async void Shaders_FileChanged(object? sender, FileSystemEventArgs e)
        {
            await StopAsync();
            await Task.Factory.StartNew(DisposeGraphics, CancellationToken.None, TaskCreationOptions.None, UIScheduler);
            await Task.Factory.StartNew(InitGraphics, CancellationToken.None, TaskCreationOptions.None, UIScheduler);
            SessionTask = RunAsync();
        }

        void DisposeGraphics()
        {
            HistogramShiftVariable.Dispose();
            HistogramShiftVariable2.Dispose();
            HistogramShader.Dispose();
            SpectrumShader.Dispose();
            MandelboxShader.Dispose();
            foreach (var state in State.Items.OfType<GraphicsState>())
            {
                state.Dispose();
            }
            State.Items.Clear();
            Graphics.Dispose();
        }

        (SampleSource, SessionOptions) Init()
        {

            var deviceName = Input.SelectedItem.ToString();
            AudioDevice device = deviceName is LoopbackEntry
                ? new Loopback()
                : new Recorder(deviceName ?? string.Empty);
            var sampleSource = new AudioInput(device);
            if (Options.SelectedObject is not SessionOptions options)
            {
                throw new InvalidCastException();
            }
            Spectrum = CreateSpectrum(device.Format.ChannelCount, options.Domain.Count);
            Histogram = CreateHistogram(device.Format.ChannelCount, options.Domain.Count, options.TimeResolution);

            SpectrumShader.ShaderResources["Spectrum0"].SetResource(Spectrum[0].Texture.GetShaderResourceView());
            SpectrumShader.ShaderResources["Spectrum1"].SetResource(Spectrum[1].Texture.GetShaderResourceView());
            MandelboxShader.ShaderResources["Spectrum0"].SetResource(Spectrum[0].Texture.GetShaderResourceView());
            MandelboxShader.ShaderResources["Spectrum1"].SetResource(Spectrum[1].Texture.GetShaderResourceView());
            MandelboxShader.ShaderResources["Histogram0"].SetResource(Histogram[0].Texture.GetShaderResourceView());
            MandelboxShader.ShaderResources["Histogram1"].SetResource(Histogram[1].Texture.GetShaderResourceView());
            HistogramShader.ShaderResources["Histogram0"].SetResource(Histogram[0].Texture.GetShaderResourceView());
            HistogramShader.ShaderResources["Histogram1"].SetResource(Histogram[1].Texture.GetShaderResourceView());

            return (sampleSource, options);

        }

        public async Task RunAsync()
        {
            var (sampleSource, options) = await Task.Factory.StartNew(Init, CancellationToken.None, TaskCreationOptions.None, UIScheduler);
            Cancel = new CancellationTokenSource();
            Session = new Session(sampleSource, options);
            Cancel.Token.Register(sampleSource.Stop);

            Session.OnSamplesAvailable += (obj, evt) =>
            {
                var latency = DateTime.Now - evt.Time;
                if (Latency < latency)
                {
                    Latency = latency;
                }
                for (int c = 0; c < sampleSource.Format.ChannelCount; ++c)
                {
                    var channel = evt.GetChannel(c);
                    Spectrum[c].Add(channel.Span);
                    Histogram[c].Add(channel.Span);
                }
                Interlocked.Increment(ref EventCounter);
            };

            if (0 < Session.Options.DMXPort)
            {
                var comPort = $"COM{Session.Options.DMXPort}";
                var dmx = new DMXController(comPort, Session.Options.Domain)
                {
                    SinuoidThreshold = Session.Options.SinuoidThreshold,
                    TransientThreshold = Session.Options.TransientThreshold,
                };
                Session.OnSamplesAvailable += (obj, evt) => dmx.Process(evt);
                Session.OnStopped += async (obj, evt) => await dmx.DisposeAsync();
            }

            Session.OnStopped += async (obj, evt) =>
            {
                await Task.Factory.StartNew(sampleSource.Dispose, CancellationToken.None, TaskCreationOptions.LongRunning, UIScheduler);
            };

            var sessionTask = Task.Run(() => Session.RunAsync(Cancel.Token), Cancel.Token);
            var renderTask = Task.Factory.StartNew(() => Render(Cancel.Token), Cancel.Token, TaskCreationOptions.LongRunning, UIScheduler);

            sampleSource.Start();

            await sessionTask;
            await renderTask;

        }

        async Task StopAsync()
        {
            Cancel.Cancel();
            try
            {
                await SessionTask;
            }
            catch(OperationCanceledException)
            { }
            foreach (var spectrum in Spectrum)
            {
                spectrum.Dispose();
            }
            foreach (var histogram in Histogram)
            {
                histogram.Dispose();
            }
        }

        void Render(CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested)
            {
                RenderFrame();
                Application.DoEvents();
            }
        }

        public void RenderFrame()
        {

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

                foreach (var histogram in Histogram)
                {
                    histogram.Update();
                }

                var histogramShift = (Histogram[0].Position - .1f) / Histogram[0].Texture.Height;
                HistogramShiftVariable.Set(histogramShift);
                HistogramShiftVariable2.Set(histogramShift);

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

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            Graphics.Resize(ClientSize.Width, ClientSize.Height);
        }

        protected async override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            await StopAsync();
            DisposeGraphics();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Graphics.Resize(ClientSize.Width, ClientSize.Height);
            SessionTask = RunAsync();
        }

        protected async override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    tlpPanel.Visible ^= true;
                    break;
                case Keys.F5:
                    await StopAsync();
                    SessionTask = RunAsync();
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

        private async void Options_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (Session != null)
            {
                await StopAsync();
                SessionTask = RunAsync();
            }
        }

        private async void Input_SelectedValueChanged(object sender, EventArgs e)
        {
            if(Session != null)
            {
                await StopAsync();
                SessionTask = RunAsync();
            }
        }

    }
}