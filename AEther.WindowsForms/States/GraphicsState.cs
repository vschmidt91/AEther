using System;
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
