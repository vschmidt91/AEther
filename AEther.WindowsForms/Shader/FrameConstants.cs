using System.Runtime.InteropServices;

namespace AEther.WindowsForms
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct FrameConstants
    {

        [FieldOffset(0)]
        public float T;

        [FieldOffset(4)]
        public float DT;

        [FieldOffset(8)]
        public float HistogramShift;

        [FieldOffset(12)]
        public float AspectRatio;


    }
}
