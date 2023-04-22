using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Utils
{
    public sealed class PriorityQueue<TElement, TPriority> : IEnumerable<TElement>, IReadOnlyCollection<TElement>
    {
        private const int DEFAULT_CAPACITY = 6;

        private readonly Func<Node, Node, int> _comparer;
        private int _nodes;
        private Node[] _heap;
        private int _count;
        private bool _isHeap;

        public PriorityQueue(int capacity, IComparer<TPriority> comparer)
        {
            _heap = new Node[capacity > 0 ? capacity : DEFAULT_CAPACITY];
            _nodes = 0;
            _count = 0;

            var priorityComparer = comparer ?? Comparer<TPriority>.Default;
            _comparer = (a, b) =>
            {
                int priority = priorityComparer.Compare(a.Priority, b.Priority);
                return priority != 0 ? priority : b.Age - a.Age;
            };
        }
        public PriorityQueue(int capacity) : this(capacity, null) { }
        public PriorityQueue(IComparer<TPriority> comparer) : this(0, comparer) { }
        public PriorityQueue() : this(0, null) { }
        public PriorityQueue(IEnumerable<ValueTuple<TElement, TPriority>> elements, IComparer<TPriority> comparer) : this(elements.Count(), comparer)
        {
            foreach (var element in elements) Enqueue(element.Item1, element.Item2);
        }
        public PriorityQueue(IEnumerable<ValueTuple<TElement, TPriority>> elements) : this(elements, null) { }

        public int Count => _count;

        public void EnsureCapacity(int capacity)
        {
            capacity = checked(capacity + _count);
            if (_heap.Length < capacity) Array.Resize(ref _heap, capacity);
        }

        public void Enqueue(TElement value, TPriority priority)
        {
            if (_count == _heap.Length)
            {
                Array.Resize(ref _heap, _count * 2);
            }
            Node x = new Node(_nodes++, value, priority);

            if (_isHeap)
            {
                SiftUp(_count, x, 0);
            }
            else
            {
                _heap[_count] = x;
            }
            ++_count;
        }

        public TElement Peek()
        {
            if (_isHeap) Heapify();
            return _heap[0] is Node ? _heap[0].Value : default;
        }

        public TElement Dequeue()
        {
            if (!_isHeap) Heapify();
            if (_count > 0)
            {
                TElement top = _heap[0].Value;
                Node bottom = _heap[--_count];
                int index = SiftDown(0);
                SiftUp(index, bottom, 0);
                _heap[_count] = default;
                return top;
            }
            return default;
        }

        public void Clear()
        {
            for (int i = 0; i < _count; ++i) _heap[i] = null;
            _count = 0;
            _isHeap = false;
        }

        public IEnumerator<TElement> GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Private members

        private int SiftDown(int index)
        {
            int parent = index;
            int leftChild = HeapLeftChild(parent);

            while (leftChild < _count)
            {
                int rightChild = HeapRightFromLeft(leftChild);
                int bestChild = (rightChild < _count && _comparer(_heap[rightChild], _heap[leftChild]) < 0)
                    ? rightChild : leftChild;

                _heap[parent] = _heap[bestChild];
                parent = bestChild;
                leftChild = HeapLeftChild(parent);
            }

            return parent;
        }

        private void SiftUp(int index, Node x, int boundary)
        {
            while (index > boundary)
            {
                int parent = HeapParent(index);
                if (_comparer(_heap[parent], x) > 0)
                {
                    _heap[index] = _heap[parent];
                    index = parent;
                }
                else break;
            }
            _heap[index] = x;
        }

        private void Heapify()
        {
            if (!_isHeap)
            {
                for (int i = _count / 2 - 1; i >= 0; --i)
                {
                    Node x = _heap[i];
                    int index = SiftDown(i);
                    SiftUp(index, x, i);
                }
                _isHeap = true;
            }
        }

        private static int HeapParent(int i) => (i - 1) / 2;
        private static int HeapLeftChild(int i) => (i * 2) + 1;
        private static int HeapRightFromLeft(int i) => i + 1;

        #endregion

        private class Node
        {
            public readonly int Age;
            public readonly TElement Value;
            public readonly TPriority Priority;

            public Node(int age, TElement value, TPriority priority) =>
                (Age, Value, Priority) = (age, value, priority);
        }

        private class Enumerator : IEnumerator<TElement>
        {
            private PriorityQueue<TElement, TPriority> _source;
            private int _i = -1;

            public Enumerator(PriorityQueue<TElement, TPriority> source)
            {
                _source = source;
            }

            public TElement Current
            {
                get
                {
                    if (_i < 0) throw new IndexOutOfRangeException();
                    if (_i >= _source._count) throw new IndexOutOfRangeException();
                    return _source._heap[_i].Value;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose() {}

            public bool MoveNext()
            {
                if (_i + 1 < _source._count)
                {
                    ++_i;
                    return true;
                }
                return false;
            }

            public void Reset() => _i = -1;
        }
    }
}
