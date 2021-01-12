using System;
using System.Collections.Generic;
using System.Text;

namespace AEther.Bass
{
    public readonly struct Device
    {

        public readonly int Index;
        public readonly string Name;

        public Device(int index, string name)
        {
            (Index, Name) = (index, name);
        }

    }
}
