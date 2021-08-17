using System.Windows.Forms;

namespace AEther.WindowsForms
{
    public class GraphicsModule : IDisposable
    {

        const bool UseMapping = true;

        readonly Graphics Graphics;
        readonly SpectrumAccumulator<float>[] Spectrum;
        readonly Histogram[] Histogram;
        readonly ListBox States;
        readonly ConstantBuffer<FrameConstants> FrameConstants;

        int EventCounter = 0;

        public GraphicsModule(Graphics graphics, ListBox states, int channelCount, int noteCount, int historyCount)
        {

            var A1 = SharpDX.Matrix.LookAtLH(SharpDX.Vector3.Zero, SharpDX.Vector3.ForwardLH, SharpDX.Vector3.Up);
            var A2 = System.Numerics.Matrix4x4.CreateLookAt(System.Numerics.Vector3.Zero, -System.Numerics.Vector3.UnitZ, System.Numerics.Vector3.UnitY);

            Graphics = graphics;

            FrameConstants = Graphics.CreateConstantBuffer<FrameConstants>();
            Graphics.ShaderConstants["FrameConstants"] = FrameConstants.Buffer;

            Spectrum = Enumerable.Range(0, channelCount)
                .Select(i => (SpectrumAccumulator<float>)new FloatSpectrum(Graphics, noteCount))
                .ToArray();

            Histogram = Enumerable.Range(0, channelCount)
                .Select(i => (Histogram)new FloatHistogram(Graphics, noteCount, historyCount, UseMapping))
                .ToArray();

            states.Items.Clear();
            states.Items.AddRange(new GraphicsState[]
            {
                new SceneState(Graphics, Spectrum),
                new FluidState(Graphics),
                new IFSState(Graphics),
                new SpectrumState(Graphics, Spectrum),
                new HistogramState(Graphics, Histogram),
            });
            states.SelectedIndex = 0;
            States = states;

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            foreach (var spectrum in Spectrum)
            {
                spectrum.Dispose();
            }
            foreach (var histogram in Histogram)
            {
                histogram.Dispose();
            }
            foreach (var state in States.Items.OfType<GraphicsState>())
            {
                state.Dispose();
            }
            FrameConstants.Dispose();
        }

        public void ProcessKeyPress(KeyPressEventArgs evt)
        {
            foreach (var state in States.Items.OfType<GraphicsState>())
            {
                state.ProcessKeyPress(evt);
            }
        }

        public void Process(SampleEvent<double> evt)
        {
            foreach (var (spectrum, c) in Spectrum.WithIndex())
            {
                var channel = evt.GetChannel(c);
                spectrum.Add(channel.Span);
            }
            foreach (var (histogram, c) in Histogram.WithIndex())
            {
                var channel = evt.GetChannel(c);
                histogram.Add(channel.Span);
            }
            Interlocked.Increment(ref EventCounter);
        }

        public void Render()
        {

            var dt = .01f;
            var t = (float)DateTime.Now.TimeOfDay.TotalSeconds;

            FrameConstants.Value = new()
            {
                AspectRatio = Graphics.BackBuffer.Width / (float)Graphics.BackBuffer.Height,
                T = t,
                DT = dt,
                HistogramShift = (float)Histogram[0].Position / Histogram[0].Length,
            };
            FrameConstants.Update();

            if (States.SelectedItem is GraphicsState state)
            {
                state.Render();
                Graphics.Present();
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
            }
        }
    }
}
