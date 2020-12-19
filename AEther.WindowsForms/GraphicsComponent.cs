using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther.WindowsForms
{
    public abstract class GraphicsComponent
    {

        public readonly Graphics Graphics;

        public GraphicsComponent(Graphics graphics)
        {
            Graphics = graphics;
        }

    }
}
