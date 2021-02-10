using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class AudioInput : SampleSource
    {

        public override SampleFormat Format => Device.Format;

        readonly AudioDevice Device;

        public bool EnableLoopDetection = true;

        byte[] HashBuffer = Array.Empty<byte>();
        readonly HashAlgorithm Hash;
        readonly ConcurrentQueue<ulong> RecentHashes = new();

        public AudioInput(AudioDevice device)
        {

            Hash = MD5.Create();
            //LastHash = new byte[(Hash.HashSize + 7) / 8];

            Device = device;
            Device.OnDataAvailable += Device_OnDataAvailable;
            Device.OnStopped += Device_OnStopped;

        }

        private void Device_OnStopped(object? sender, Exception? error)
        {
            OnStopped?.Invoke(this, error);
        }

        private void Device_OnDataAvailable(object? sender, ReadOnlyMemory<byte> samples)
        {

            if(EnableLoopDetection)
            {

                if (HashBuffer.Length < samples.Length)
                {
                    HashBuffer = new byte[samples.Length];
                }
                samples.CopyTo(HashBuffer);

                while (8 < RecentHashes.Count)
                {
                    RecentHashes.TryDequeue(out _);
                }
                var hash = Hash.ComputeHash(HashBuffer, 0, samples.Length);
                var hashLong = BitConverter.ToUInt64(hash);
                var loopDetected = RecentHashes.Contains(hashLong);
                if(loopDetected)
                {
                    return;
                }
                else
                {
                    RecentHashes.Enqueue(hashLong);
                }

            }

            OnDataAvailable?.Invoke(this, samples);

        }

        public override void Start()
        {
            Device.Start();
        }

        public override void Stop()
        {
            Device.Stop();
            OnStopped?.Invoke(this, null);
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            Device.Dispose();
        }

    }
}
