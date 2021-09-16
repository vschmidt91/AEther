using System;

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
