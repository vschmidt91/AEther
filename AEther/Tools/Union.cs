using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class Union<X, Y>
    {

        public X First
        {
            get => Data.Item1;
            set { Data = (value, default); Flag = 0; }
        }

        public Y Second
        {
            get => Data.Item2;
            set { Data = (default, value); Flag = 1; }
        }

        private (X, Y) Data;
        private int Flag;
        
        public Z Map<Z>(Func<X, Z> f1, Func<Y, Z> f2)
        {
            switch(Flag)
            {
                case 0: return f1(First);
                case 1: return f2(Second);
                default: return default;
            }
        }

    }
    public class Union<X, Y, Z>
    {

        public X First
        {
            get => Data.Item1;
            set { Data = (value, default, default); Flag = 0; }
        }

        public Y Second
        {
            get => Data.Item2;
            set { Data = (default, value, default); Flag = 1; }
        }

        public Z Third
        {
            get => Data.Item3;
            set { Data = (default, default, value); Flag = 2; }
        }

        private (X, Y, Z) Data;
        private int Flag;

        public W Map<W>(Func<X, W> f1, Func<Y, W> f2, Func<Z, W> f3)
        {
            switch (Flag)
            {
                case 0: return f1(First);
                case 1: return f2(Second);
                case 2: return f3(Third);
                default: return default;
            }
        }


    }
}
