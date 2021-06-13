using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public interface ISessionModule : IDisposable
    {

        void Process(SampleEvent<double> samples);

        void Render();

    }
}
