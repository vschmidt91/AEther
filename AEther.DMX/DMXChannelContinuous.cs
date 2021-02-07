using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther.DMX
{
    public class DMXChannelContinuous : DMXChannel
    {

        public double Value;

        public override byte ByteValue => (byte)(255 * Value);

    }
}
