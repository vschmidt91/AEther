using System;
using System.Collections.Generic;
using System.Text;

namespace AEther
{
    public interface IFrequencyFilter<T>
    {

        void Filter(ReadOnlySpan<T> spectrum, Memory<T> result);

        void Clear();

    }
}
