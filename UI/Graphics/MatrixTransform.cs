using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Graphics
{
    public class MatrixTransform : IVectorF
    {
        private IVectorF _A;
        private IVectorF _B;
        private int _VecCount;
        private Func<int, float, float, float> _TransformFunc;

        public MatrixTransform(IVectorF a, IVectorF b, Func<int, float, float, float> transformFunc)
        {
            _A = a;
            _B = b;
            _VecCount = a.VecCount;
            if (_VecCount != b.VecCount)
                throw new ArgumentException("The vectors are not of equal length");
            _TransformFunc = transformFunc;

        }

        public float this[int index] => _TransformFunc(index, _A[index], _B[index]);
        public int VecCount => _VecCount;
    }

    public class IterativeTransform : IVectorF
    {
        public int VecCount => _VecCount;

        public IterativeTransform(IEnumerable<IVectorF> vecs, Func<int, float, float, float> iterativeTransformFunc)
        {
            var first = vecs.FirstOrDefault();
            if (first == null)
                throw new ArgumentException(nameof(vecs));
            _FirstVec = first;
            _Vecs = vecs.Skip(1);
            _VecCount = first.VecCount;
            _IterativeTransformFunc = iterativeTransformFunc;
        }

        private IEnumerable<IVectorF> _Vecs;
        private IVectorF _FirstVec;
        private int _VecCount;
        private Func<int, float, float, float> _IterativeTransformFunc;

        public float this[int index]
        {
            get
            {
                float iterVal = _FirstVec[index];
                int n = 0;
                foreach (var vec in _Vecs)
                {
                    if (vec.VecCount != _VecCount)
                        throw new InvalidOperationException("The collection contained vectors of inconsistent length");
                    iterVal = _IterativeTransformFunc(n, iterVal, vec[index]);
                    n++;
                }
                return iterVal;
            }
        }
    }

    public class UniformTransform : IVectorF
    {
        private IVectorF _Vec;
        private Func<int, float, float> _TransformFunc;

        public UniformTransform(IVectorF vec, Func<int, float, float> transformFunc)
        {
            _Vec = vec;
            _TransformFunc = transformFunc;
        }

        public float this[int index] => _TransformFunc(index, _Vec.VecCount);
        public int VecCount => _Vec.VecCount;
    }

    public class ArrayVector : IVectorF
    {
        private float[] _Array;

        public ArrayVector(params float[] values)
        {
            _Array = values;
        }

        public float this[int index] => _Array[index];
        public int VecCount => _Array.Length;
    }
}
