using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace AEther.WindowsForms
{

    public class FluidState : GraphicsState
    {

        readonly Fluid Fluid;

        public FluidState(Graphics graphics)
            : base(graphics)
        {
            int size = 1 << 10;
            Fluid = new Fluid(Graphics, size, size);
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            Fluid.Dispose();
        }

        public override void Render()
        {
            Fluid.Render();
        }

    }
}
