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
using System.Threading;

namespace AEther.CLI
{
    class Program
    {
        static async Task<int> Main(string? path = null)
        {

            path = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "TestFiles", "test_input.wav");

            var options = new SessionOptions();
            
            SampleSource sampleSource;
            if (path == null)
                sampleSource = new CSCore.Loopback();
            else
                sampleSource = new SampleReader(File.OpenRead(path));

            var session = new Session(sampleSource.Format, options);

            sampleSource.OnDataAvailable += (sender, evt) =>
            {
                session.Post(evt);
            };
            sampleSource.OnStopped += (sender, evt) =>
            {
                session.Complete();
            };

            sampleSource.Start();

            while (!session.Completion.IsCompleted)
            {
                SampleEvent output;
                try
                {
                    output = await session.ReceiveAsync(CancellationToken.None);
                }
                catch (InvalidOperationException) { break; }
                catch (TaskCanceledException) { break; }
                //Console.WriteLine(JsonSerializer.Serialize(output));
                session.Pool.Return(output.Samples);
            }

            sampleSource.Dispose();

            return 0;

        }

    }
}
