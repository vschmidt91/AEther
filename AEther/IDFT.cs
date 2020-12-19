using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AEther
{

    public interface IDFT
    {

        IEnumerable<DFTEvent> Filter(SampleEvent sampleEvent);

        void Clear();

    }

}