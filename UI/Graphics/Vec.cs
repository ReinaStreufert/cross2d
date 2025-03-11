using SharpDX;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Graphics
{
    public static class Vec
    {
        public static IVectorF Uniform(IVectorF a, Func<int, float, float> transformFunc) =>
            new UniformTransform(a, transformFunc);
        public static TVec Uniform<TVec>(TVec a, Func<int, float, float> transformFunc) where TVec : IVectorF<TVec> =>
            a.CreateFrom(Uniform((IVectorF)a, transformFunc));
        public static IVectorF Matrix(IVectorF a, IVectorF b, Func<int, float, float, float> transformFunc) =>
            new MatrixTransform(a, b, transformFunc);
        public static TVec Matrix<TVec>(TVec a, IVectorF b, Func<int, float, float, float> transformFunc) where TVec : IVectorF<TVec> =>
            a.CreateFrom(Matrix(a, b, transformFunc));
        public static IVectorF Iterative(IEnumerable<IVectorF> vecs, Func<int, float, float, float> transformFunc) =>
            new IterativeTransform(vecs, transformFunc);
        public static TVec Iterative<TVec>(IEnumerable<TVec> vecs, Func<int, float, float, float> transformFunc) where TVec : IVectorF<TVec> =>
            vecs.First().CreateFrom(Iterative(vecs, transformFunc));

        public static IVectorF Add(IVectorF a, IVectorF b) => Matrix(a, b, (i, ax, bx) => ax + bx);
        public static TVec Add<TVec>(TVec a, IVectorF b) where TVec : IVectorF<TVec> => Matrix(a, b, (i, ax, bx) => ax + bx);
        public static IVectorF Subtract(IVectorF a, IVectorF b) => Matrix(a, b, (i, ax, bx) => ax - bx);
        public static TVec Subtract<TVec>(TVec a, IVectorF b) where TVec : IVectorF<TVec> => Matrix(a, b, (i, ax, bx) => ax - bx);
        public static IVectorF Sum(IEnumerable<IVectorF> vecs) => Iterative(vecs, (i, t, x) => t + x);
        public static TVec Sum<TVec>(IEnumerable<TVec> vecs) where TVec : IVectorF<TVec> => Iterative(vecs, (i, t, x) => t + x);
    }
}
