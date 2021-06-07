using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public abstract class WindowedFilter<T> : MovingFilter<T>
    {

        public readonly int WindowSize;

        public WindowedFilter(T state, int windowSize)
            : base(state)
        {
            WindowSize = windowSize;
        }

    }
}
