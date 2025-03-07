using Cross.UI.Graphics;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public class SpatialUnit<TVec> where TVec : IVectorF<TVec>
    {
        public static SpatialUnit<TVec> Uniform(TVec vec, SpatialRelativity relativity)
        {
            var relativityArr = new SpatialRelativity[vec.VecCount];
            for (int i = 0; i < relativityArr.Length; i++)
                relativityArr[i] = relativity;
            return new SpatialUnit<TVec>(vec, relativityArr);
        }

        public static SpatialUnit<TVec> Absolute(TVec vec) => Uniform(vec, SpatialRelativity.Absolute);
        public static SpatialUnit<TVec> RelativeTotal(TVec vec) => Uniform(vec, SpatialRelativity.RelativeTotal);
        public static SpatialUnit<TVec> RelativeRemaining(TVec vec) => Uniform(vec, SpatialRelativity.RelativeRemaining);

        public TVec Value { get; }
        public SpatialRelativity[] Relativity { get; }

        public SpatialUnit(TVec value, params SpatialRelativity[] relativity)
        {
            Value = value;
            if (relativity.Length != value.VecCount)
                throw new ArgumentException($"There are not as many relativity values are there are coordinate");
            Relativity = relativity;
        }

        public TVec ToAbsolute(TVec totalSpace, TVec remainingSpace)
        {
            return Value.CreateFrom(new UniformTransform(Value, (i, value) =>
            {
                var relativity = Relativity[i];
                return relativity switch
                {
                    SpatialRelativity.Absolute => value,
                    SpatialRelativity.RelativeRemaining => remainingSpace[i] * value,
                    SpatialRelativity.RelativeTotal => totalSpace[i] * value,
                    _ => throw new NotImplementedException()
                };
            }));
        }

        public static TVec GetSpaceRemaining(TVec totalSpace, IEnumerable<SpatialUnit<TVec>> vectors)
        {
            var resultVecs = new float[totalSpace.VecCount];
            for (int i = 0; i < resultVecs.Length; i++)
                resultVecs[i] = totalSpace[i];
            foreach (var unit in vectors)
            {
                if (unit.Value.VecCount != resultVecs.Length)
                    throw new ArgumentException($"Vector length is inconsistent");
                for (int i = 0; i < resultVecs.Length; i++)
                {
                    var relativity = unit.Relativity[i];
                    var vec = unit.Value[i];
                    resultVecs[i] -= relativity switch
                    {
                        SpatialRelativity.Absolute => vec,
                        SpatialRelativity.RelativeTotal => vec * totalSpace[i],
                        SpatialRelativity.RelativeRemaining => 0f,
                        _ => throw new NotImplementedException()
                    };
                }
            }
            return totalSpace.CreateFrom(new ArrayVector(resultVecs));
        }
    }

    public enum SpatialRelativity
    {
        Absolute,
        RelativeTotal,
        RelativeRemaining
    }
}
