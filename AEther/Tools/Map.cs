using System;
using System.Collections.Generic;
using System.Text;

namespace AEther
{
    public class Map<T>
        where T : class
    {

        public const int NumBits = 5;
        public static readonly int NumNodes = 1 << NumBits;
        public static readonly int Mask = NumNodes - 1;

        object[] Nodes;

        public T this[int p]
        {
            get => Get(p);
            set => Set(p, value);
        }

        public T Get(int p)
        {
            object[] node = Nodes;
            for(int bits = 0; bits < 32; bits += NumBits)
            {
                if (node is null)
                    return default;
                node = node[p & Mask] as object[];
                p >>= NumBits;
            }
            return node[p & Mask] as T;
        }

        public void Set(int p, T value)
        {
            object[] node = Nodes ?? (Nodes = new object[NumNodes]);
            for (int bits = 0; bits < 32; bits += NumBits)
            {
                int i = p & Mask;
                if (node[i] is null)
                    node[i] = new object[NumNodes];
                node = node[i] as object[];
                p >>= NumBits;
            }
            node[p] = value;
        }

    }
}
