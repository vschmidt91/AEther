using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AEther.WindowsForms
{
    public abstract class GraphicsState : GraphicsComponent, IDisposable
    {

        public GraphicsState(Graphics graphics)
            : base(graphics)
        { }

        public abstract void Dispose();

        public abstract void Render();

        public override string ToString()
        {
            return GetType().Name;
        }

        public virtual void ProcessKeyPress(KeyPressEventArgs evt)
        { }

    }
}
