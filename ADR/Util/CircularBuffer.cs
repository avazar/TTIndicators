using System;
using System.Collections;
using System.Collections.Generic;

namespace ADR.Util
{
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private T[] _buffer;
        private readonly int _capacity;
        private int _head;
        private int _tail;

        public int Capacity => _capacity;

        public int Count { get; private set; }

        public T this[int index]
        {
            get
            {
                if (index >= Count) throw new IndexOutOfRangeException();
                return _buffer[GetInternalIndex(index)];
            }
            set
            {
                if (index >= Count) throw new IndexOutOfRangeException();
                _buffer[GetInternalIndex(index)] = value;
            }
        }

        public CircularBuffer(int capacity)
        {
            _capacity = capacity;
            _buffer = new T[capacity];
            _head = 0;
            _tail = 0;
            Count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _buffer[GetInternalIndex(i)];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int GetNextIndex(int current)
        {
            var next = current + 1;
            return next < _capacity ? next : _capacity - next;
        }

        private int GetInternalIndex(int index)
        {
            var internalIndex = _head + index;
            return internalIndex < _capacity ? internalIndex : internalIndex - _capacity;
        }

        public void Push(T obj)
        {
            _buffer[_tail] = obj;
            if (Count == Capacity)
            {
                _head = GetNextIndex(_head);
            }
            else
            {
                Count++;
            }
            _tail = GetNextIndex(_tail);
        }

        public T Pop()
        {
            if (Count == 0) return default(T);
            var obj = _buffer[_head];
            _head = GetNextIndex(_head);
            Count--;
            return obj;
        }

        public T Peek()
        {
            if (Count == 0) return default(T);
            return _buffer[_head];
        }


    }
}
