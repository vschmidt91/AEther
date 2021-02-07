using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther.DMX
{
    public class DMXChannelDiscreteThresholds : DMXChannelContinuous
    {

        readonly (byte value, double threshold)[] Items;

        public DMXChannelDiscreteThresholds((byte, double)[] items)
        {
            Items = items;
        }

        //public override byte ByteValue => Items[Array.FindLastIndex(Items, t => t.threshold <= Value)].value;
        public override byte ByteValue => Array.FindLastIndex(Items, t => t.threshold <= Value) switch
        {
            -1 => Items[^1].value,
            int i => Items[i].value,
        };

    }
}
