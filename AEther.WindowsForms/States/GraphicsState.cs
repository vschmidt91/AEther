using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther.WindowsForms
{
    public abstract class GraphicsState : GraphicsComponent
    {

        public GraphicsState(Graphics graphics)
            : base(graphics)
        {

        }

        public abstract void Render();

        public override string ToString()
        {
            return GetType().Name;
        }

    }
}
