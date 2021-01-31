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
        double Latency = 0;
        DateTime LastLatencyUpdate = DateTime.MinValue;
        int InputCounter = 0;

        readonly Graphics Graphics;
        readonly Shader HistogramShader;
        readonly Shader SpectrumShader;
        readonly TimeSpan LatencyUpdateInterval = TimeSpan.FromSeconds(1);
        readonly EffectScalarVariable HistogramShiftVariable;
        readonly TaskScheduler UIScheduler;

        readonly DMXController DMX;

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

            DMX = new DMXController("COM4");

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

            string sampleSourceName = LoopbackEntry;
            SessionOptions? options = null;

            void Init()
            {
                sampleSourceName = (string)Input.SelectedItem;
                options = (SessionOptions)Options.SelectedObject;
                Spectrum = CreateSpectrum();
                Histogram = CreateHistogram();
            }

            await Task.Factory.StartNew(Init, Cancel.Token, TaskCreationOptions.None, UIScheduler);

            using SampleSource sampleSource = sampleSourceName is LoopbackEntry
                ? new Loopback()
                : new Recorder(sampleSourceName);

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

                    DMX.Process(output);
                    for (int c = 0; c < sampleSource.Format.ChannelCount; ++c)
                    {
                        var src = output.GetChannel(c);
                        lock (Spectrum[c])
                        {
                            Spectrum[c].Add(src.Span);
                        }
                        lock (Histogram[c])
                        {
                            Histogram[c].Add(src.Span);
                        }
                    }

                    InputCounter++;
                    output.Dispose();
                }
            }
            catch(OperationCanceledException) { }
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
                Text = $"Latency: {Math.Round(Latency, 1)} ms";
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

        SpectrumAccumulator[] CreateSpectrum()
        {

            var options = (SessionOptions)Options.SelectedObject;
            var domainLength = options.Domain.Count;
            var spectrum = new SpectrumAccumulator[2];

            for (var c = 0; c < spectrum.Length; ++c)
            {
                spectrum[c] = UseFloatTextures
                    ? new FloatSpectrum(Graphics, domainLength)
                    : new ByteSpectrum(Graphics, domainLength);
            }

            //foreach (var key in Graphics.Shaders.Keys)
            //{
            //    var shader = Graphics.Shaders[key];
            //    for (var c = 0; c < spectrum.Length; ++c)
            //    {
            //        if (shader.ShaderResources.TryGetValue("Spectrum" + c, out var variable))
            //        {
            //            variable.SetResource(spectrum[c].Texture.GetShaderResourceView());
            //        }
            //    }
            //}

            return spectrum;
        }

        Histogram[] CreateHistogram()
        {

            var options = (SessionOptions)Options.SelectedObject;
            var domainLength = options.Domain.Count;
            var histogramLength = options.TimeResolution;

            var histogram = new Histogram[2];

            for (var c = 0; c < histogram.Length; ++c)
            {
                histogram[c] = UseFloatTextures
                    ? new FloatHistogram(Graphics, domainLength, histogramLength, UseMapping)
                    : new ByteHistogram(Graphics, domainLength, histogramLength, UseMapping);
            }

            //foreach (var key in Graphics.Shaders.Keys)
            //{
            //    var shader = Graphics.Shaders[key];
            //    for (var c = 0; c < histogram.Length; ++c)
            //    {
            //        if (shader.ShaderResources.TryGetValue("Histogram" + c, out var variable))
            //        {
            //            variable.SetResource(histogram[c].Texture.GetShaderResourceView());
            //        }
            //    }
            //}

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
