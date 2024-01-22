using System.Collections;


namespace Elevator.Common.Types
{
    public class ConcurrentList<T> : IList<T>
    {
        private readonly List<T> _innerList = new();
        private readonly object _syncRoot = new();

        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _innerList.Count;
                }
            }
        }

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get
            {
                lock (_syncRoot)
                {
                    return _innerList[index];
                }
            }
            set
            {
                lock (_syncRoot)
                {
                    _innerList[index] = value;
                }
            }
        }

        public void Add(T item)
        {
            lock (_syncRoot)
            {
                _innerList.Add(item);
            }
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                _innerList.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (_syncRoot)
            {
                return _innerList.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_syncRoot)
            {
                _innerList.CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_syncRoot)
            {
                return _innerList.GetEnumerator();
            }
        }

        public int IndexOf(T item)
        {
            lock (_syncRoot)
            {
                return _innerList.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (_syncRoot)
            {
                _innerList.Insert(index, item);
            }
        }

        public bool Remove(T item)
        {
            lock (_syncRoot)
            {
                return _innerList.Remove(item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_syncRoot)
            {
                _innerList.RemoveAt(index);
            }
        }

        public int RemoveAll(Predicate<T> match)
        {
            lock (_syncRoot)
            {
                return _innerList.RemoveAll(match);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_syncRoot)
            {
                return ((IEnumerable)_innerList).GetEnumerator();
            }
        }
    }
}
