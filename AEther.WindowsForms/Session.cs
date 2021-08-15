using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AEther.DMX;

namespace AEther.WindowsForms
{
    public class Session : IDisposable
    {

        public readonly MainForm Form;
        public readonly StatisticsModule Statistics;
        public readonly SampleSource Source;
        public readonly Analyzer Analyzer;
        public readonly DMXController DMX;
        public readonly GraphicsModule GraphicsModule;

        public Session(MainForm form, SampleSource source, ListBox states, AnalyzerOptions options, DMXOptions dmxOptions)
        {
            Form = form;
            Statistics = new StatisticsModule(form);
            Source = source;
            Analyzer = new(source.Format, options);

            GraphicsModule = new GraphicsModule(form.Graphics, states, Source.Format.ChannelCount, options.Domain.Length, options.TimeResolution);
            DMX = new DMXController(options.Domain, dmxOptions);
            Analyzer.SamplesAnalyzed += Analyzer_SamplesAnalyzed;
            Source.DataAvailable += Source_DataAvailable;
            Form.KeyPress += Form_KeyPress;

        }

        private void Form_KeyPress(object? sender, KeyPressEventArgs e)
        {
            GraphicsModule.ProcessKeyPress(e);
        }

        private void Source_DataAvailable(object? sender, ReadOnlyMemory<byte> evt)
        {
            Analyzer.PostSamples(evt);
        }

        private void Analyzer_SamplesAnalyzed(object? sender, SampleEvent<double> evt)
        {
            Statistics.Process(evt);
            DMX.Process(evt);
            GraphicsModule.Process(evt);
        }

        public void Start()
        {
            Source.Start();
        }

        public void Render()
        {
            Statistics.Update();
            GraphicsModule.Render();
        }

        public async Task StopAsync()
        {

            Analyzer.SamplesAnalyzed -= Analyzer_SamplesAnalyzed;
            Source.DataAvailable -= Source_DataAvailable;
            Form.KeyPress -= Form_KeyPress;
            Source.Stop();
            Source.Dispose();
            Analyzer.Stop();
            try
            {
                await Analyzer.Completion;
            }
            catch (OperationCanceledException)
            { }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Source.Dispose();
            GraphicsModule.Dispose();
            DMX.Dispose();
        }

    }
}
