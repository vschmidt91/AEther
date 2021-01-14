using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class DataEvent : IDisposable
    {

        static ArrayPool<byte> Pool => ArrayPool<byte>.Shared;

        public readonly byte[] Data;
        public readonly int Length;
        public readonly DateTime Time;

        public DataEvent(int length, DateTime time)
        {
            Data = Pool.Rent(length);
            Length = length;
            Time = time;
        }

        public void Dispose()
        {
            Pool.Return(Data);
        }

    }
}
