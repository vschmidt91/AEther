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

            var options = new SessionOptions();
            
            SampleSource sampleSource;
            if (path == null)
                sampleSource = new CSCore.Loopback();
            else
                sampleSource = new SampleReader(File.OpenRead(path));

            var pipe = new System.IO.Pipelines.Pipe();
            var session = new Session(sampleSource.Format, options);
            var chain = session.CreateBatcher()
                .Chain(session.CreateDFT())
                .Chain(session.CreateSplitter());

            sampleSource.OnDataAvailable += async (sender, evt) =>
            {
                await pipe.Writer.WriteAsync(evt);
            };

            var inputs = pipe.Reader.ReadAllAsync();
            sampleSource.Start();
            await foreach (var output in chain(inputs))
            {
                Console.WriteLine(JsonSerializer.Serialize(output));
                session.Pool.Return(output.Samples);
            }

            return 0;

        }

    }
}
