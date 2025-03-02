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
        public TVec RemainingSpace
        {
            get
            {
                if (_CachedRemainingSpace == null)
                    _CachedRemainingSpace = SpatialUnit<TVec>.GetSpaceRemaining(TotalSpace, _Claimed);
                return _CachedRemainingSpace;
            }
        }

        public SpatialContext(TVec totalSpace)
        {
            TotalSpace = totalSpace;
        }

        private TVec? _CachedRemainingSpace;
        private HashSet<SpatialUnit<TVec>> _Claimed = new HashSet<SpatialUnit<TVec>>();

        public void Claim(SpatialUnit<TVec> space)
        {
            _Claimed.Add(space);
            _CachedRemainingSpace = null;
        }

        public void Unclaim(SpatialUnit<TVec> space)
        {
            _Claimed.Remove(space);
            _CachedRemainingSpace = null;
        }

        public TVec ToAbsolute(SpatialUnit<TVec> unit)
        {
            return unit.ToAbsolute(TotalSpace, RemainingSpace);
        }
    }
}
