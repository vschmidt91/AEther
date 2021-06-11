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
using AEther.NAudio;

using SharpDX.Direct3D11;
using System.Threading.Tasks.Dataflow;
using System.DirectoryServices;
using System.Reflection;

namespace AEther.WindowsForms
{
    public partial class MainForm : Form
    {

        internal record ActiveSession
        (
            SampleSource SampleSource,
            Session Session
        )
        {

            internal void Start()
            {
                SampleSource.Start();
            }

            async internal Task StopAsync()
            {
                SampleSource.Stop();
                await Session.StopAsync();
            }

        }

        const string LoopbackEntry = "Loopback";
        const bool UseMapping = true;
        const bool UseFloatTextures = false;
        public static readonly TimeSpan LatencyUpdateInterval = TimeSpan.FromSeconds(1);

        readonly Stopwatch FrameTimer = Stopwatch.StartNew();
        readonly Stopwatch LatencyUpdateTimer = Stopwatch.StartNew();
        readonly Graphics Graphics;

        SpectrumAccumulator[] Spectrum = Array.Empty<SpectrumAccumulator>();
        Histogram[] Histogram = Array.Empty<Histogram>();
        ActiveSession? Session = null;
        TimeSpan Latency = TimeSpan.Zero;
        TimeSpan FrameTime = TimeSpan.Zero;
        int EventCounter = 0;

        public MainForm()
        {

            InitializeComponent();

            Graphics = new Graphics(Handle);
            InitStates();

            var audioDevices = Recorder.GetAvailableDeviceNames();
            Input.Items.Clear();
            Input.Items.Add(LoopbackEntry);
            Input.Items.AddRange(audioDevices.ToArray());
            Input.SelectedItem = LoopbackEntry;

            Options.SelectedObject = new SessionOptions();

        }

        async Task ResetGraphicsAsync()
        {
            await StopAsync();
            this.InvokeIfRequired(DisposeStates);
            this.InvokeIfRequired(InitStates);
            Start();
        }

        void InitStates()
        {

            State.Items.Clear();
            State.Items.AddRange(new GraphicsState[]
            {
                new SceneState(Graphics),
                new HistogramState(Graphics, Histogram),
                new SpectrumState(Graphics, Spectrum),
                new FluidState(Graphics),
                new IFSState(Graphics),
            });
            State.SelectedIndex = 0;
            Graphics.Shaders.FileChanged += Shaders_FileChanged;

        }

        private async void Shaders_FileChanged(object? sender, FileSystemEventArgs e)
        {
            await ResetGraphicsAsync();
        }

        void DisposeStates()
        {
            foreach (var state in State.Items.OfType<GraphicsState>())
            {
                state.Dispose();
            }
            State.Items.Clear();
        }

        (SampleSource, SessionOptions) Init()
        {

            var deviceName = Input.SelectedItem.ToString();
            SampleSource sampleSource = deviceName is LoopbackEntry
                ? new Loopback()
                : new Recorder(deviceName ?? string.Empty);
            if (Options.SelectedObject is not SessionOptions options)
            {
                throw new InvalidCastException();
            }

            Spectrum = CreateSpectrum(sampleSource.Format.ChannelCount, options.Domain.Length);
            Histogram = CreateHistogram(sampleSource.Format.ChannelCount, options.Domain.Length, options.TimeResolution);

            return (sampleSource, options);

        }

        public void Start()
        {

            var (sampleSource, options) = this.InvokeIfRequired(Init);
            var session = new Session(sampleSource, options);

            session.OnSamplesAvailable += (obj, evt) =>
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

            if (0 < options.DMXPort)
            {
                var comPort = $"COM{session.Options.DMXPort}";
                var dmx = new DMXController(comPort, options.Domain)
                {
                    SinuoidThreshold = options.SinuoidThreshold,
                    TransientThreshold = options.TransientThreshold,
                };
                session.OnSamplesAvailable += (obj, evt) => dmx.Process(evt);
                session.OnStopped += async (obj, evt) => await dmx.DisposeAsync();
            }

            sampleSource.OnDataAvailable += (obj, evt) =>
            {
                session.PostSamples(evt);
            };

            Session = new ActiveSession(sampleSource, session);
            Session.Start();

        }

        async Task StopAsync()
        {
            try
            {
                await (Session?.StopAsync() ?? Task.CompletedTask);
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

        public void Render()
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
                Graphics.FrameConstants.Value.HistogramShift = histogramShift;

            }

            if (State.SelectedItem is GraphicsState state)
            {
                Graphics.RenderFrame();
                state.Render();
                Graphics.Present();
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
            DisposeStates();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Graphics.Resize(ClientSize.Width, ClientSize.Height);
            Start();
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
                    Start();
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
                Start();
            }
        }

        private async void Input_SelectedValueChanged(object sender, EventArgs e)
        {
            if(Session != null)
            {
                await StopAsync();
                Start();
            }
        }

        private async void ResetGraphics_Click(object sender, EventArgs e)
        {
            await ResetGraphicsAsync();
        }
    }
}