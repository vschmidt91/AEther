﻿using System;
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
        static async Task Main(string path)
        {

            path = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "TestFiles", "test_input.wav");
            path = new FileInfo(path).FullName;

            var options = new SessionOptions();

            using var inputStream = Console.OpenStandardInput();
            using var outputStream = new MemoryStream();
            //using var standardOutput = Console.OpenStandardOutput();
            using var sampleSource = new SampleReader(File.OpenRead(path));

            var session = new Session(sampleSource, options);
            var hash = MD5.Create();

            var outputs = session.RunAsync();

            sampleSource.Start();

            await foreach (var output in outputs)
            {
                var byteCount = sizeof(float) * output.SampleCount;
                var buffer = ArrayPool<byte>.Shared.Rent(byteCount);
                Buffer.BlockCopy(output.Samples, 0, buffer, 0, byteCount);

                Console.WriteLine(BitConverter.ToString(hash.ComputeHash(buffer, 0, byteCount)));
                await outputStream.WriteAsync(buffer.AsMemory(0, byteCount));

                ArrayPool<byte>.Shared.Return(buffer);

                output.Dispose();
            }

        }

    }
}
