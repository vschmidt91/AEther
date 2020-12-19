using System;
using System.Collections.Generic;
using System.Text;

namespace AEther
{
    public abstract class TimeFilter<T>
    {

        public abstract T Filter(T x);

        public abstract void Clear();

    }
}
