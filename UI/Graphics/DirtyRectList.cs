using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Graphics
{
    public class DirtyRectList : IEnumerable<Rect2DF>
    {
        private List<Rect2DF> DirtyRects = new List<Rect2DF>();

        public DirtyRectList()
        {

        }

        public IEnumerable<Rect2DF> FindIntersections(Rect2DF rect)
        {
            return DirtyRects
                .Where(d => VectorMath.Intersects(d, rect))
                .Select(d => VectorMath.Intersection(d, rect));
        }

        public void Dirty(Rect2DF rect)
        {
            IEnumerable<Rect2DF> newRects = First(rect);
            foreach (var existingRect in DirtyRects)
                newRects = BreakUpRects(newRects, existingRect);
            DirtyRects.AddRange(newRects.ToArray());
        }

        private static IEnumerable<Rect2DF> BreakUpRects(IEnumerable<Rect2DF> rects, Rect2DF existingRect)
        {
            foreach (var rect in rects)
            {
                if (VectorMath.Intersects(rect, existingRect))
                {
                    foreach (var diffRect in VectorMath.Difference(rect, existingRect))
                        yield return diffRect;
                }
                else yield return rect;
            }
        }

        private IEnumerable<Rect2DF> First(Rect2DF rect)
        {
            yield return rect;
        }

        public IEnumerator<Rect2DF> GetEnumerator() => DirtyRects.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => DirtyRects.GetEnumerator();
    }
}
