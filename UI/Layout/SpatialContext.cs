using Cross.UI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public class SpatialContext<TVec> where TVec : class, IVectorF<TVec>
    {
        public TVec TotalSpace { get; }
        public TVec RemainingSpace => Vec.Subtract(TotalSpace, _SpaceUsed);
        public TVec SpaceUsed => _SpaceUsed;

        public SpatialContext(TVec totalSpace)
        {
            TotalSpace = totalSpace;
            var vecArr = new float[TotalSpace.VecCount];
            _SpaceUsed = totalSpace.CreateFrom(new ArrayVector(vecArr));
        }

        private TVec _SpaceUsed;

        public void Claim(SpatialUnit<TVec> space)
        {
            var effectiveSpace = GetEffectiveSpace(space);
            _SpaceUsed = Vec.Add(_SpaceUsed, effectiveSpace);
        }

        public void Claim(IEnumerable<SpatialUnit<TVec>> spaces)
        {
            var effectiveSpaces = spaces
                .Select(GetEffectiveSpace)
                .Prepend(_SpaceUsed);
            _SpaceUsed = TotalSpace.CreateFrom(Vec.Sum(effectiveSpaces));
        }

        public bool CanClaim(SpatialUnit<TVec> space)
        {
            var effectiveSpace = GetEffectiveSpace(space);
            return CanClaim(effectiveSpace);
        }

        public bool CanClaim(IEnumerable<SpatialUnit<TVec>> spaces)
        {
            var effectiveSpaces = spaces
                .Select(GetEffectiveSpace)
                .Prepend(_SpaceUsed);
            var effectiveSpace = Vec.Sum(effectiveSpaces);
            return CanClaim(effectiveSpace);
        }

        private bool CanClaim(IVectorF vec)
        {
            var remainingSpace = RemainingSpace;
            for (int i = 0; i < vec.VecCount; i++)
            {
                if (vec[i] > remainingSpace[i])
                    return false;
            }
            return true;
        }

        public TVec ToAbsolute(SpatialUnit<TVec> unit)
        {
            return unit.ToAbsolute(TotalSpace, RemainingSpace);
        }

        private IVectorF GetEffectiveSpace(SpatialUnit<TVec> unit)
        {
            return Vec.Uniform((IVectorF)(unit.Value), (i, v) => unit.Relativity[i] switch
            {
                SpatialRelativity.Absolute => v,
                SpatialRelativity.RelativeTotal => TotalSpace[i] * v,
                SpatialRelativity.RelativeRemaining => 0,
                _ => throw new NotImplementedException()
            });
        }
    }
}
