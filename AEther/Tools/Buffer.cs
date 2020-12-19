using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class Buffer<T> : IList<T>
    {

        public T[] Values;
        public int Capacity
        {
            get => Values.Length;
            set => Resize(value);
        }

        int _Size;
        public int Size
        {
            get => _Size;
            set
            {
                if (value > Capacity)
                    Resize(Math.Max(value, 2 * Capacity));
                _Size = value;
            }
        }

        public int Count => Size;

        public bool IsReadOnly => false;

        public T this[int i]
        {
            get => Values[i];
            set => Values[i] = value;
        }

        public Buffer(int capacity)
        {
            Values = new T[capacity];
        }

        public void Resize(int capacity)
        {
            var values = new T[capacity];
            Array.Copy(Values, values, Math.Min(Values.Length, values.Length));
            Values = values;
        }
        
        public IEnumerator<T> GetEnumerator()
            => Values.Take(Size).GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
        
        public int IndexOf(T item)
            => Array.IndexOf(Values, item);
        
        public void Insert(int index, T item)
        {
            Add(Values[index]);
            Values[index] = item;
        }
        
        public void RemoveAt(int index)
            => Values[index] = Values[--Size];
        
        public void Add(T item)
        {
            Size++;
            Values[Size - 1] = item;
        }
        
        public void Clear()
        {
            Array.Clear(Values, 0, Capacity);
            Size = 0;
        }
        
        public bool Contains(T item)
            => IndexOf(item) == -1;
        
        public void CopyTo(T[] array, int arrayIndex)
            => Array.Copy(Values, 0, array, arrayIndex, Math.Min(Size, array.Length - arrayIndex));
        
        public bool Remove(T item)
        {
            int i = IndexOf(item);
            if (i == -1) return false;
            RemoveAt(i);
            return true;
        }
    }
}
