using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class Bijection<T, S> : IDictionary<T, S>
    {

        readonly Dictionary<T, S> _Forward = new Dictionary<T, S>();
        readonly Dictionary<S, T> _Inverse = new Dictionary<S, T>();

        public IReadOnlyDictionary<S, T> Inverse => _Inverse;

        public S this[T key]
        {
            get
            {
                return _Forward[key];
            }
            set
            {
                Add(key, value);
            }
        }

        public ICollection<T> Keys => ((IDictionary<T, S>)_Forward).Keys;

        public ICollection<S> Values => ((IDictionary<T, S>)_Forward).Values;

        public int Count => _Forward.Count;

        public bool IsReadOnly => ((IDictionary<T, S>)_Forward).IsReadOnly;

        public void Add(T key, S value)
        {
            if (_Forward.TryGetValue(key, out S oldValue))
            {
                if (value?.Equals(oldValue) ?? true)
                    return;
                _Forward.Remove(key);
                _Inverse.Remove(oldValue);
            }
            _Forward.Add(key, value);
            _Inverse.Add(value, key);
        }

        public void Add(KeyValuePair<T, S> item) => Add(item.Key, item.Value);

        public void Clear()
        {
            _Forward.Clear();
            _Inverse.Clear();
        }

        public bool Contains(KeyValuePair<T, S> item)
        {
            return ((IDictionary<T, S>)_Forward).Contains(item);
        }

        public bool ContainsKey(T key)
        {
            return _Forward.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<T, S>[] array, int arrayIndex)
        {
            ((IDictionary<T, S>)_Forward).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<T, S>> GetEnumerator()
        {
            return ((IDictionary<T, S>)_Forward).GetEnumerator();
        }

        public bool Remove(T key)
        {
            if (_Forward.TryGetValue(key, out S oldValue))
            {
                _Forward.Remove(key);
                _Inverse.Remove(oldValue);
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<T, S> item)
        {
            return ((IDictionary<T, S>)_Forward).Remove(item);
        }

        public bool TryGetValue(T key, out S value)
        {
            return _Forward.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<T, S>)_Forward).GetEnumerator();
        }
    }
}
