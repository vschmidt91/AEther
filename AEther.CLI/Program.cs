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
        static async Task<int> Main(string? address = null, int? device = null)
        {

            var configuration = new Configuration();
            var sampleSource = new CSCore.Listener(device);

            var domain = configuration.Domain;
            var format = sampleSource.Format;
            //using var sampleSource = new SampleReader(Console.OpenStandardInput(), new byte[1 << 10]);

            var port = 1100;
            using var client = string.IsNullOrEmpty(address)
                ? null
                : new UdpClient(address, port);

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
