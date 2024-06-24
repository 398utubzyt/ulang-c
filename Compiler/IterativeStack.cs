using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ulang
{
    public class IterativeStack<T> : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ICollection
    {
        private T[] _buffer;
        private nuint _count;

        public T this[nuint index] { get { return _buffer[index]; } set { _buffer[index] = value; } }
        public T this[int index] { get { return _buffer[index]; } set { _buffer[index] = value; } }
        public T this[Index index] { get { return _buffer.AsSpan()[index]; } set { _buffer[index] = value; } }
        public Span<T> this[Range range] { get { return _buffer.AsSpan()[range]; } }

        public Span<T> AsSpan() => _buffer.AsSpan();

        public int Count => (int)_count;
        public bool IsEmpty => _count == 0;

        public bool IsSynchronized => false;
        public object SyncRoot => this;

        public bool IsReadOnly => false;

        private const nuint DefaultCapacity = 4;

        private void Grow(nuint to)
        {
            Debug.Assert((nuint)_buffer.LongLength < to);

            to = _buffer.LongLength == 0 ? DefaultCapacity : 2 * (nuint)_buffer.LongLength;

            if (to > (nuint)Array.MaxLength) to = (nuint)Array.MaxLength;
            
            Array.Resize(ref _buffer, (int)to);
        }

        public nuint EnsureCapacity(nuint capacity)
        {
            if ((nuint)_buffer.LongLength < capacity)
            {
                Grow(capacity);
            }

            return (nuint)_buffer.LongLength;
        }

        public void PushBack(T item)
        {
            if (_count >= (nuint)_buffer.LongLength)
            {
                Grow(_count + 1);
            }

            _buffer[_count++] = item;
        }

        public void PushFront(T item)
        {
            if (_count >= (nuint)_buffer.LongLength)
            {
                Grow(_count + 1);
            }

            _buffer.AsSpan()[..(int)_count++].CopyTo(_buffer.AsSpan()[1..(int)_count]);
            _buffer[0] = item;
        }

        public T Peek()
        {
            nuint index = _count - 1;

            if (index >= _count)
            {
                throw new InvalidOperationException("Cannot peek on an empty stack.");
            }

            return _buffer[index];
        }

        public bool TryPeek(out T result)
        {
            nuint index = _count - 1;

            if (index >= _count)
            {
                result = default;
                return false;
            }

            result = _buffer[index];
            return true;
        }

        public T PopBack()
        {
            nuint index = _count - 1;

            if (index >= _count)
            {
                throw new InvalidOperationException("Cannot pop on an empty stack.");
            }

            _count = index;
            T result = _buffer[index];
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _buffer[index] = default;
            }
            return result;
        }

        public bool TryPopBack(out T result)
        {
            nuint index = _count - 1;

            if (index >= _count)
            {
                result = default;
                return false;
            }

            _count = index;
            result = _buffer[index];
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _buffer[index] = default;
            }
            return true;
        }

        public T PopFront()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Cannot pop on an empty stack.");
            }

            T result = _buffer[0];

            if (_count != 1)
                _buffer.AsSpan()[1..(int)_count].CopyTo(_buffer.AsSpan()[0..((int)_count - 1)]);
            --_count;

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _buffer[_count] = default;
            }

            return result;
        }

        public bool TryPopFront(out T result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }

            result = _buffer[0];

            if (_count != 1)
                _buffer.AsSpan()[1..(int)_count].CopyTo(_buffer.AsSpan()[0..((int)_count - 1)]);
            --_count;

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _buffer[_count] = default;
            }

            return true;
        }

        public void Clear()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(_buffer, 0, (int)_count);
            }
            _count = 0;
        }

        public bool Contains(T item)
        {
            return _count != 0 && Array.LastIndexOf(_buffer, item) != -1;
        }

        void ICollection.CopyTo(Array array, int index)
            => _buffer.CopyTo(array, index);

        public void CopyTo(T[] array, int arrayIndex)
        {
            _buffer.AsSpan(0, (int)_count).CopyTo(array.AsSpan(arrayIndex));
        }

        public IEnumerator<T> GetEnumerator()
            => new Enumerator(this);
        public IEnumerator<T> GetReverseEnumerator()
            => new ReverseEnumerator(this);
        public IEnumerator<T> GetPopEnumerator()
            => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator()
            => new Enumerator(this);

        private class Enumerator(IterativeStack<T> stack) : IEnumerator<T>
        {
            private readonly IterativeStack<T> _stack = stack;
            private nuint _index = unchecked((nuint)(-1));

            public T Current =>
                _index < _stack._count ? _stack._buffer[_index] :
                throw new IndexOutOfRangeException("Current enumerator position is out of bounds.");
            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return ++_index < _stack._count;
            }

            public void Reset()
            {
                _index = unchecked((nuint)(-1));
            }
        }
        private class ReverseEnumerator(IterativeStack<T> stack) : IEnumerator<T>
        {
            private readonly IterativeStack<T> _stack = stack;
            private nuint _index = unchecked((nuint)(-1));

            public T Current =>
                _index < _stack._count ? _stack._buffer[_index] :
                throw new IndexOutOfRangeException("Current enumerator position is out of bounds.");
            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return --_index < _stack._count;
            }

            public void Reset()
            {
                _index = _stack._count;
            }
        }
        private class PopEnumerator(IterativeStack<T> stack) : IEnumerator<T>
        {
            private readonly IterativeStack<T> _stack = stack;
            private T _value;

            public T Current => _value;
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _value = default;
            }

            public bool MoveNext()
            {
                return _stack.TryPopBack(out _value);
            }

            public void Reset()
            {
            }
        }

        public IterativeStack()
        {
            _buffer = Array.Empty<T>();
        }

        public IterativeStack(nuint capacity)
        {
            _buffer = new T[capacity];
        }

        public IterativeStack(IEnumerable<T> collection)
        {
            _buffer = collection.ToArray();
        }

        public IterativeStack(ReadOnlySpan<T> buffer)
        {
            _buffer = new T[buffer.Length];
            buffer.CopyTo(_buffer.AsSpan());
        }
    }
}
