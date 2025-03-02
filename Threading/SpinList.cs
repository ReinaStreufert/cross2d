using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.Threading
{
    // a fast, concurrent, memory efficient, unordered list which can be added to and removed from
    public class SpinList<T> : IEnumerable<T> where T : class
    {
        private const int InitialCapacity = 16;
        private const int GrowFactor = 2;

        public SpinList()
        {
            _Elements = new Element[InitialCapacity];
            _FreeIndices = new int[InitialCapacity];
            _FreeListWrapStart = 0;
            _FreeListWrapEnd = InitialCapacity - 1;
            _Capacity = InitialCapacity;
            for (int i = 0; i < InitialCapacity; i++)
                _FreeIndices[i] = i;
        }

        private Element[] _Elements;
        private int[] _FreeIndices;
        private int _FreeListWrapStart;
        private int _FreeListWrapEnd;
        private int _Capacity;
        private object _Lock = new object();

        public int Add(T item)
        {
            lock (_Lock)
            {
                int id = Rotate(ref _FreeListWrapStart);
                if (id == _FreeListWrapEnd)
                    Grow();
                ref Element dst = ref _Elements[id];
                dst.Value = item;
                dst.Occupied = true;
                return id;
            }
        }

        public bool Remove(int itemId, T expectedVal)
        {
            ref Element el = ref _Elements[itemId];
            lock (_Lock)
            {
                if (!el.Occupied || el.Value != expectedVal)
                    return false;
                el.Occupied = false;
                int freeIdx = Rotate(ref _FreeListWrapEnd);
                _FreeIndices[freeIdx] = itemId;
                return true;
            }
        }

        // this method will not function correctly if the free list is not completely full
        public void Grow()
        {
            int oldCapacity = _Capacity;
            int newCapacity = oldCapacity * GrowFactor;
            Array.Resize(ref _Elements, newCapacity);
            Array.Resize(ref _FreeIndices, newCapacity);
            var freeFillIndex = _FreeListWrapEnd;
            _FreeListWrapEnd = (_FreeListWrapStart + newCapacity) % newCapacity;
            _Capacity = newCapacity;
            var makeupCount = newCapacity - oldCapacity;
            for (int i = oldCapacity; i < makeupCount; i++)
            {
                var idx = Rotate(ref freeFillIndex);
                _FreeIndices[idx] = i;
            }
        }

        private int Rotate(ref int idx)
        {
            int oldVal = idx;
            if (idx >= _Capacity - 1)
                idx = 0;
            else
                idx = oldVal + 1;
            return idx;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Iterate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Iterate().GetEnumerator();
        }

        private IEnumerable<T> Iterate()
        {
            int capacity;
            Element[] itemArr;
            lock (_Lock)
            {
                capacity = _Capacity;
                itemArr = _Elements;
            }
            for (int i = 0; i < capacity; i++)
            {
                Element el = itemArr[i];
                if (el.Occupied)
                    yield return el.Value;
            }
        }

        private struct Element
        {
            public bool Occupied;
            public T Value;

            public Element(T value)
            {
                Occupied = true;
                Value = value;
            }
        }
    }
}
