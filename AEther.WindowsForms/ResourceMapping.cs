using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;

using SharpDX;
using SharpDX.Direct3D11;

namespace AEther.WindowsForms
{
    public class ContextMapping : IDisposable
    {

        public long Length => Stream?.Length ?? 0;
        public long Pitch => Box.RowPitch;

        readonly DeviceContext Context;
        readonly Resource Resource;
        readonly int Subresource;
        readonly DataBox Box;

        DataStream Stream;

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

        public void WriteRange<T>(Span<T> span)
            where T : struct
        {
            var buffer = ArrayPool<T>.Shared.Rent(span.Length);
            span.CopyTo(buffer);
            Stream.WriteRange(buffer, 0, span.Length);
        }

        public void Dispose()
        {
            Stream.Close();
            Utilities.Dispose(ref Stream);
            Context.UnmapSubresource(Resource, Subresource);
        }

    }
}
