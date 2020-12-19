using System;
using System.Collections.Generic;
using System.Text;

namespace AEther
{
    public interface ITimeFilter<T>
    {

        T Filter(T x);

        void Clear();

    }
}
