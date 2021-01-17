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

using AEther.CSCore;

namespace AEther.CLI
{
    class Program
    {
        static async Task<int> Main(string? path = null)
        {

            path = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "TestFiles", "test_input.wav");
            path = new FileInfo(path).FullName;

            var options = new SessionOptions();
            
            SampleSource sampleSource;
            if (path == null)
                sampleSource = new CSCore.Loopback();
            else
                sampleSource = new SampleReader(File.OpenRead(path));

            var session = new Session(sampleSource.Format, options);

            sampleSource.OnDataAvailable += (sender, data) =>
            {
                var evt = new DataEvent(data.Length, DateTime.Now);
                data.CopyTo(evt.Data);
                session.Writer.TryWrite(evt);
            };
            sampleSource.OnStopped += (sender, evt) =>
            {
                session.Writer.TryComplete();
            };

            var sessionTask = Task.Run(() => session.RunAsync());
            sampleSource.Start();

            await foreach (var output in session.Reader.ReadAllAsync())
            {
                //Console.WriteLine(JsonSerializer.Serialize(output));
                output.Dispose();
            }
            await sessionTask;

            sampleSource.Dispose();

            return 0;

        }

    }
}
