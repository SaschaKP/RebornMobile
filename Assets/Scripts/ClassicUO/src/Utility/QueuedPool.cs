

using System;
using System.Collections.Generic;

namespace ClassicUO.Utility
{
    internal class QueuedPool<T> where T : class, new()
    {
        public readonly Stack<T> _pool;
        private readonly Action<T> _on_pickup;
        private readonly int _maxSize;


        public QueuedPool(int size, Action<T> onpickup = null)
        {
            _maxSize = size;
            _pool = new Stack<T>(size);
            _on_pickup = onpickup;

            for (int i = 0; i < size; i++)
                _pool.Push(new T());
        }


        public int MaxSize => _maxSize;
        public int Remains => _maxSize - _pool.Count;

        public T GetOne()
        {
            T result;

            if (_pool.Count != 0)
            {
                result = _pool.Pop();
                _on_pickup?.Invoke(result);
            }
            else
            {
                result = new T();
            }

            return result;
        }

        public void ReturnOne(T obj)
        {
            if (obj != null)
            {
                _pool.Push(obj);
            }
        }

        public void Clear()
        {
            _pool.Clear();
        }
    }
}