using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace AEther
{
    public enum SampleType
    {
        Unknown,
        Float32,
        UInt16,
    }

    public static class SampleTypeExt
    {

        public static int GetSize(this SampleType format)
            => format switch
            {
                SampleType.Float32 => 4,
                SampleType.UInt16 => 2,
                _ => 0,
            };

    }

}
