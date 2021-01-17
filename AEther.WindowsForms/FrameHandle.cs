using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther.WindowsForms
{
    public class FrameHandle : GraphicsComponent, IDisposable
    {

        public FrameHandle(Graphics graphics)
            : base(graphics)
        { }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Graphics.Present();
        }

    }

}
