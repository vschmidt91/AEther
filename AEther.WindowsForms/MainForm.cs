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

namespace AEther.WindowsForms
{
    public partial class MainForm : Form
    {

        const string LoopbackEntry = "Loopback";
        const bool UseMapping = true;
        const bool UseFloatTextures = false;

        readonly Graphics Graphics;
        readonly bool IsInitialized;

        Task Task;
        CancellationTokenSource Cancel;
        SpectrumAccumulator[] Spectrum;
        Histogram[] Histogram;
        double Latency = 0;
        readonly TimeSpan LatencyUpdateInterval = TimeSpan.FromSeconds(1);
        DateTime LastLatencyUpdate = DateTime.MinValue;
        int InputCounter = 0;


        Shader HistogramShader => Graphics.Shaders["histogram.fx"];
        Shader SpectrumShader => Graphics.Shaders["spectrum.fx"];
        EffectScalarVariable HistogramShiftVariable;

        public MainForm()
        {

            InitializeComponent();
            Graphics = new Graphics(Handle);

            HistogramShiftVariable = HistogramShader.Variables["HistogramShift"].AsScalar();

            var audioDevices = Recorder.GetAvailableDeviceNames();
            lbInput.Items.Clear();
            lbInput.Items.Add(LoopbackEntry);
            lbInput.Items.AddRange(audioDevices.ToArray());
            lbInput.SelectedItem = LoopbackEntry;

            lbState.Items.Clear();
            lbState.Items.AddRange(new GraphicsState[]
            {
                new ShaderState(Graphics, HistogramShader),
                new ShaderState(Graphics, SpectrumShader),
                new FluidState(Graphics),
                new IFSState(Graphics),
            });
            lbState.SelectedIndex = 3;

            pgOptions.SelectedObject = new SessionOptions();

            IsInitialized = true;
            Spectrum = CreateSpectrum();
            Histogram = CreateHistogram();

            Cancel = new CancellationTokenSource();
            Task = RunAsync(Cancel.Token);

        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            Graphics?.Resize(ClientSize.Width, ClientSize.Height);
        }

        protected async override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Cancel.Cancel();
            await Task;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Graphics.Resize(ClientSize.Width, ClientSize.Height);
        }

        protected async override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    TogglePanel();
                    break;
                case Keys.F11:
                    ToggleFullscreen();
                    break;
                case Keys.Escape:
                    Cancel.Cancel();
                    await Task;
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


        async Task RunAsync(CancellationToken cancel = default)
        {

            if (lbInput.SelectedItem is not string sampleSourceName)
                return;

            if (pgOptions.SelectedObject is not SessionOptions options)
                return;

            SampleSource sampleSource = sampleSourceName is LoopbackEntry
                ? new Loopback()
                : new Recorder(sampleSourceName);

            var format = sampleSource.Format;
            var session = new Session(format, options);

            void dataAvailable(object? sender, ReadOnlyMemory<byte> data)
            {
                session.Post(data);
            }

            void stopped(object? sender, Exception? exc)
            {
                session.Complete();
            }

            sampleSource.OnDataAvailable += dataAvailable;
            sampleSource.OnStopped += stopped;
            cancel.Register(sampleSource.Stop);
            sampleSource.Start();

            while (!session.Completion.IsCompleted)
            {
                SampleEvent output;
                try
                {
                    output = await session.ReceiveAsync(cancel);
                }
                catch (InvalidOperationException) { break; }
                catch (TaskCanceledException) { break; }
                var latency = (DateTime.Now - output.Time).TotalMilliseconds;
                Latency = Math.Max(Latency, latency);
                for (int c = 0; c < format.ChannelCount; ++c)
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
                session.Pool.Return(output.Samples);
                }

            sampleSource.Dispose();

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
                        spectrum.Update(Graphics.Context);
                        spectrum.Clear();
                    }
                }
                foreach (var histogram in Histogram)
                {
                    lock (histogram)
                    {
                        histogram.Update(Graphics.Context);
                    }
                }
                var histogramShift = (Histogram[0].Position - .1f) / Histogram[0].Texture.Height;
                HistogramShiftVariable.Set(histogramShift);
                InputCounter = 0;
            }

            var state = lbState.SelectedItem as GraphicsState;
            using (var frame = Graphics.RenderFrame())
            {
                state?.Render();
            }

        }

        SpectrumAccumulator[] CreateSpectrum()
        {

            var options = (SessionOptions)pgOptions.SelectedObject;
            var domainLength = options.Domain.Count;
            var spectrum = new SpectrumAccumulator[2];

            for (var c = 0; c < spectrum.Length; ++c)
            {
                spectrum[c] = UseFloatTextures
                    ? new FloatSpectrum(Graphics.Device, domainLength)
                    : new ByteSpectrum(Graphics.Device, domainLength);
            }

            foreach (var key in Graphics.Shaders.Keys)
            {
                var shader = Graphics.Shaders[key];
                for (var c = 0; c < spectrum.Length; ++c)
                {
                    if (shader.ShaderResources.TryGetValue("Spectrum" + c, out var variable))
                    {
                        variable.SetResource(spectrum[c].Texture.GetShaderResourceView());
                    }
                }
            }

            return spectrum;
        }

        Histogram[] CreateHistogram()
        {

            var options = (SessionOptions)pgOptions.SelectedObject;
            var domainLength = options.Domain.Count;
            var histogramLength = options.TimeResolution;

            var histogram = new Histogram[2];

            for (var c = 0; c < histogram.Length; ++c)
            {
                histogram[c] = UseFloatTextures
                    ? new FloatHistogram(Graphics.Device, domainLength, histogramLength, UseMapping)
                    : new ByteHistogram(Graphics.Device, domainLength, histogramLength, UseMapping);
            }

            foreach (var key in Graphics.Shaders.Keys)
            {
                var shader = Graphics.Shaders[key];
                for (var c = 0; c < histogram.Length; ++c)
                {
                    if (shader.ShaderResources.TryGetValue("Histogram" + c, out var variable))
                    {
                        variable.SetResource(histogram[c].Texture.GetShaderResourceView());
                    }
                }
            }

            return histogram;

        }

        private async void pgOptions_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {

            Cancel.Cancel();
            await Task;

            SpectrumAccumulator[] oldSpectrum;
            (Spectrum, oldSpectrum) = (CreateSpectrum(), Spectrum);
            foreach (var spectrum in oldSpectrum)
            {
                spectrum.Dispose();
            }

            Histogram[] oldHistogram;
            (Histogram, oldHistogram) = (CreateHistogram(), Histogram);
            foreach (var histogram in oldHistogram)
            {
                histogram.Dispose();
            }

            Cancel = new CancellationTokenSource();
            Task = RunAsync(Cancel.Token);

        }

        private async void lbInput_SelectedValueChanged(object sender, EventArgs e)
        {

            if (!IsInitialized)
                return;

            Cancel.Cancel();
            await Task;

            Cancel = new CancellationTokenSource();
            Task = RunAsync(Cancel.Token);

        }
    }
}
