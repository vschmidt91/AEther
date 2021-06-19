﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AEther
{
    public class Session
    {

        public readonly SampleSource Source;
        public readonly Analyzer Analyzer;
        public readonly List<ISessionModule> Modules = new();

        public Session(SampleSource source, AnalyzerOptions options)
        {
            Source = source;
            Analyzer = new Analyzer(Source.Format, options);
        }

        public void Start()
        {
            Analyzer.OnSamplesAvailable += Analyzer_OnSamplesAvailable;
            Source.OnDataAvailable += Source_OnDataAvailable;
            Source.Start();
        }

        void Source_OnDataAvailable(object? sender, ReadOnlyMemory<byte> evt)
        {
            Analyzer.PostSamples(evt);
        }

        public async Task StopAsync()
        {
            try
            {
                await Analyzer.StopAsync();
            }
            catch (OperationCanceledException)
            { }
            Source.Stop();
        }

        public void Render()
        {
            foreach (var module in Modules)
            {
                module.Render();
            }
        }

        void Analyzer_OnSamplesAvailable(object? sender, SampleEvent<double> evt)
        {
            foreach (var module in Modules)
            {
                module.Process(evt);
            }
        }

    }
}
