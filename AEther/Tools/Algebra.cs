using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public abstract class Algebra<T, X>
        where X : Algebra<T, X>, new()
    {

        public T Data;
        
        public IEnumerable<X> Yield()
        {
            yield return (X)this;
        }

    }
}
