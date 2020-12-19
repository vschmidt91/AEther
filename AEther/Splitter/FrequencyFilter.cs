using System;
using System.Collections.Generic;
using System.Text;

namespace AEther
{
    public abstract class FrequencyFilter<T> : TimeFilter<T>
    {

        public abstract void FilterSpan(ReadOnlySpan<T> spectrum, Span<T> result);

    }
}
