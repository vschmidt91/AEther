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
        double Latency;
        DateTime LastLatencyUpdate = DateTime.MinValue;
        int InputCounter;

        readonly Graphics Graphics;
        readonly Shader HistogramShader;
        readonly Shader SpectrumShader;
        readonly TimeSpan LatencyUpdateInterval = TimeSpan.FromSeconds(1);
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

            SessionOptions? options = null;
            SampleSource? sampleSource = null;

            void Init()
            {
                var sampleSourceName = Input.SelectedItem.ToString();
                sampleSource = sampleSourceName is LoopbackEntry
                    ? new Loopback()
                    : new Recorder(sampleSourceName);

                options = Options.SelectedObject as SessionOptions;
                Spectrum = CreateSpectrum(sampleSource.Format.ChannelCount, options.Domain.Count);
                Histogram = CreateHistogram(sampleSource.Format.ChannelCount, options.Domain.Count, options.TimeResolution);
            }

            await Task.Factory.StartNew(Init, Cancel.Token, TaskCreationOptions.None, UIScheduler);

            var session = new Session(sampleSource, options ?? new());

            Cancel.Token.Register(sampleSource.Stop);
            var outputs = session.RunAsync();

            IsRunning = true;
            sampleSource.Start();
            try
            {
                await foreach (var output in outputs)
                {
                    var latency = (DateTime.Now - output.Time).TotalMilliseconds;
                    Latency = Math.Max(Latency, latency);
                    for (int c = 0; c < Spectrum.Length; ++c)
                    {
                        var src = output.GetChannel(c);
                        lock (Spectrum[c])
                        {
                            Spectrum[c].Add(src.Span);
                        }
                    }
                    for (int c = 0; c < Histogram.Length; ++c)
                    {
                        var src = output.GetChannel(c);
                        lock (Histogram[c])
                        {
                            Histogram[c].Add(src.Span);
                        }
                    }
                    InputCounter++;
                    output.Dispose();
                }
            }
            catch(Exception) { }

            sampleSource.Dispose();
            IsRunning = false;

        }

        void ToggleFullscreen()
        {
            if(Graphics.IsFullscreen)
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

            var now = DateTime.Now;
            if (LatencyUpdateInterval < now - LastLatencyUpdate)
            {
                Text = $"Latency: {Math.Round(Latency, 0)} ms";
                LastLatencyUpdate = now;
                Latency = 0;
            }

            if (0 < InputCounter)
            {

                foreach (var spectrum in Spectrum)
                {
                    lock(spectrum)
                    {
                        spectrum.Update();
                        spectrum.Clear();
                    }
                }
                SpectrumShader.ShaderResources["Spectrum0"].SetResource(Spectrum[0].Texture.GetShaderResourceView());
                SpectrumShader.ShaderResources["Spectrum1"].SetResource(Spectrum[1].Texture.GetShaderResourceView());

                foreach (var histogram in Histogram)
                {
                    lock (histogram)
                    {
                        histogram.Update();
                    }
                }
                HistogramShader.ShaderResources["Histogram0"].SetResource(Histogram[0].Texture.GetShaderResourceView());
                HistogramShader.ShaderResources["Histogram1"].SetResource(Histogram[1].Texture.GetShaderResourceView());

                var histogramShift = (Histogram[0].Position - .1f) / Histogram[0].Texture.Height;
                HistogramShiftVariable.Set(histogramShift);
                InputCounter = 0;
            }

            if(State.SelectedItem is GraphicsState state)
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
