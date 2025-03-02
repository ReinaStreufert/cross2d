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
