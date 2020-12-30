using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

using System.CommandLine;
using System.Buffers;

namespace AEther.CLI
{
    class Program
    {
        static async Task<int> Main(string? path = null)
        {

            var configuration = new Configuration();
            // var sampleSource = 
            
            SampleSource sampleSource;
            if (path == null)
                sampleSource = new CSCore.WASAPI();
            else
                sampleSource = new SampleReader(File.OpenRead(path));

            var domain = configuration.Domain;
            var format = sampleSource.Format;

            var pipe = new System.IO.Pipelines.Pipe();
            var session = new Session(configuration, sampleSource.Format);
            var chain = session.CreateBatcher()
                .Chain(session.CreateDFT())
                .Chain(session.CreateSplitter());

            var writerTask = sampleSource.WriteToAsync(pipe.Writer);
            var inputs = pipe.Reader.ReadAllAsync();

            await foreach (var output in chain(inputs))
            {
                Console.WriteLine(JsonSerializer.Serialize(output));
                output.Return();
            }

            await writerTask;

            return 0;

        }

    }
}
