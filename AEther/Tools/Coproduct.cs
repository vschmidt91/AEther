using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AEther
{

    public struct Coproduct<T1, T2>
    {
        
        T1 _First;
        T2 _Second;
        int Tag;

        public Coproduct(T1 first)
        {
            _First = first;
            _Second = default;
            Tag = 1;
        }

        public Coproduct(T2 second)
        {
            _First = default;
            _Second = second;
            Tag = 2;
        }

        public T1 First
        {
            get => _First;
            set
            {
                Tag = 1;
                _First = value;
            }
        }

        public T2 Second
        {
            get => _Second;
            set
            {
                Tag = 2;
                _Second = value;
            }
        }

        public Coproduct<S1, S2> Map<S1, S2>(Func<T1, S1> f1, Func<T2, S2> f2)
        {
            switch (Tag)
            {
                case 1: return new Coproduct<S1, S2>(f1(_First));
                case 2: return new Coproduct<S1, S2>(f2(_Second));
                default: throw new Exception();
            }
        }

        public S Map<S>(Func<T1, S> f1, Func<T2, S> f2)
        {
            switch (Tag)
            {
                case 1: return f1(_First);
                case 2: return f2(_Second);
                default: throw new Exception();
            }
        }


    }
}
