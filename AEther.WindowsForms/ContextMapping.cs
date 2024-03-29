﻿
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.IO;

namespace AEther.WindowsForms
{
    public class ContextMapping : IDisposable
    {

        public long Length => Stream.Length;
        public long Pitch => Box.RowPitch;

        readonly Resource Resource;
        readonly int Subresource;
        readonly DataBox Box;
        readonly DataStream Stream;

        protected bool IsDisposed;

        public ContextMapping(Resource resource, int subresource = 0, MapMode mode = MapMode.WriteDiscard, MapFlags flags = MapFlags.None)
        {
            Resource = resource;
            Subresource = subresource;
            Box = Resource.Device.ImmediateContext.MapSubresource(Resource, Subresource, mode, flags, out Stream);
        }

        public void Write(ReadOnlySpan<byte> buffer)
        {
            ((Stream)Stream).Write(buffer);
        }

        public void Write<T>(T value)
            where T : struct
        {
            Stream.Write(value);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            Stream.Write(buffer, offset, count);
        }

        public void WriteRange<T>(T[] values, int offset, int count)
            where T : struct
        {
            Stream.WriteRange(values, offset, count);
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Stream.Close();
                Stream.Dispose();
                Resource.Device.ImmediateContext.UnmapSubresource(Resource, Subresource);
                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }

    }
}
