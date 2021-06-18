using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AEther.WindowsForms
{
    public class GraphicsModule : ISessionModule
    {

        const bool UseFloatTextures = false;
        const bool UseMapping = true;

        readonly Graphics Graphics;
        readonly SpectrumAccumulator[] Spectrum;
        readonly Histogram[] Histogram;
        readonly ListBox States;

        int EventCounter = 0;

        public GraphicsModule(Graphics graphics, ListBox states, int channelCount, int noteCount, int historyCount)
        {

            var A1 = SharpDX.Matrix.LookAtLH(SharpDX.Vector3.Zero, SharpDX.Vector3.ForwardLH, SharpDX.Vector3.Up);
            var A2 = System.Numerics.Matrix4x4.CreateLookAt(System.Numerics.Vector3.Zero, -System.Numerics.Vector3.UnitZ, System.Numerics.Vector3.UnitY);

            Graphics = graphics;
            Spectrum = Enumerable.Range(0, channelCount)
                .Select<int, SpectrumAccumulator>(i => UseFloatTextures
                    ? new FloatSpectrum(Graphics, noteCount)
                    : new ByteSpectrum(Graphics, noteCount))
                .ToArray();
                
            Histogram = Enumerable.Range(0, channelCount)
                .Select<int, Histogram>(i => UseFloatTextures
                    ? new FloatHistogram(Graphics, noteCount, historyCount, UseMapping)
                    : new ByteHistogram(Graphics, noteCount, historyCount, UseMapping))
                .ToArray();
            states.Items.Clear();
            states.Items.AddRange(new GraphicsState[]
            {
                new SceneState(Graphics),
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
            foreach(var state in States.Items.OfType<GraphicsState>())
            {
                state.Dispose();
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

        public void ProcessKeyPress(KeyPressEventArgs evt)
        {
            foreach (var state in States.Items.OfType<GraphicsState>())
            {
                state.ProcessKeyPress(evt);
            }
        }

        public void Process(SampleEvent<double> evt)
        {
            foreach(var (spectrum, c) in Spectrum.WithIndex())
            {
                var channel = evt.GetChannel(c);
                spectrum.Add(channel.Span);
                Histogram[c].Add(channel.Span);
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
                Graphics.FrameConstants.Value.HistogramShift = (Histogram[0].Position - .1f) / Histogram[0].Texture.Height;
            }
            if (States.SelectedItem is GraphicsState state)
            {
                Graphics.RenderFrame();
                state.Render();
                Graphics.Present();
            }
        }
    }
}
