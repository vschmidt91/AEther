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
using System.Security.Cryptography;

namespace AEther.CLI
{
    class Program
    {
        static void Main(string path)
        {

            //path = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "TestFiles", "test_input.wav");
            //path = new FileInfo(path).FullName;

            var options = new AnalyzerOptions();

            using var inputStream = Console.OpenStandardInput();
            using var outputStream = new MemoryStream();
            using var standardOutput = Console.OpenStandardOutput();
            using var sampleSource = new WAVReader(File.OpenRead(path));

            var session = new Analyzer(sampleSource.Format, options);
            var hash = MD5.Create();
            byte[] buffer = Array.Empty<byte>();
            var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            session.OnSamplesAvailable += async (obj, evt) =>
            {
                var byteCount = sizeof(double) * evt.SampleCount;
                if (buffer.Length < byteCount)
                {
                    buffer = new byte[byteCount];
                }
                Buffer.BlockCopy(evt.Samples, 0, buffer, 0, byteCount);

                Console.WriteLine(BitConverter.ToString(hash.ComputeHash(buffer, 0, byteCount)));
                await outputStream.WriteAsync(buffer.AsMemory(0, byteCount));
            };
            session.OnStopped += (obj, evt) => waitHandle.Set();

            sampleSource.Start();
            waitHandle.WaitOne();

        }

    }
}
