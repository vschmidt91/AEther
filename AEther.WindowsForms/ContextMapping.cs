using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;

using SharpDX;
using SharpDX.Direct3D11;

namespace AEther.WindowsForms
{
    public readonly struct ContextMapping
    {

        public long Length => Stream?.Length ?? 0;
        public long Pitch => Box.RowPitch;

        readonly DeviceContext Context;
        readonly Resource Resource;
        readonly int Subresource;
        readonly DataBox Box;
        readonly DataStream Stream;

        public ContextMapping(DeviceContext context, Resource resource, int? subresource = default, MapMode? mode = default, MapFlags? flags = default)
        {
            Context = context;
            Resource = resource;
            Subresource = subresource ?? 0;
            Box = Context.MapSubresource(Resource, Subresource, mode ?? MapMode.WriteDiscard, flags ?? MapFlags.None, out Stream);
        }

        public void Write(ReadOnlySpan<byte> buffer)
        {
            ((System.IO.Stream)Stream).Write(buffer);
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
            Stream.Close();
            Stream.Dispose();
            Context.UnmapSubresource(Resource, Subresource);
        }

    }
}
