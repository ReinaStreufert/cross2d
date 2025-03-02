using SharpDX;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Graphics
{
    public static class VectorMath
    {
        public static bool Intersects(this Rect2DF rect1, Rect2DF rect2)
        {
            return rect1.Right >= rect2.Left && rect1.Left <= rect2.Right &&
                rect1.Bottom >= rect2.Top && rect1.Top <= rect2.Bottom;
        }

        public static Rect2DF Intersection(this Rect2DF rect1, Rect2DF rect2)
        {
            return new Rect2DF(
                Math.Max(rect1.Left, rect2.Left),
                Math.Min(rect1.Right, rect2.Right),
                Math.Max(rect1.Top, rect2.Top),
                Math.Min(rect1.Bottom, rect2.Bottom));
        }

        public static IEnumerable<Rect2DF> Difference(this Rect2DF rect1, Rect2DF rect2)
        {
            var verticalMinLeft = rect1.Left;
            var verticalMaxRight = rect1.Right;
            // left clip
            if (rect1.Left < rect2.Left)
            {
                yield return new Rect2DF(rect1.Left, rect1.Top, rect2.Left - 1, rect1.Bottom);
                verticalMinLeft = rect2.Left - 1;
            }
            // right clip
            if (rect1.Right > rect2.Right)
            {
                yield return new Rect2DF(rect2.Right + 1, rect1.Top, rect1.Right, rect1.Bottom);
                verticalMaxRight = rect2.Right + 1;
            }
            var verticalLeft = Math.Max(rect1.Left, verticalMinLeft);
            var verticalRight = Math.Min(rect1.Right, verticalMaxRight);
            // top clip
            if (rect1.Top < rect2.Top)
                yield return new Rect2DF(verticalLeft, rect1.Top, verticalRight, rect2.Top - 1);
            if (rect1.Bottom > rect2.Bottom)
                yield return new Rect2DF(verticalLeft, rect2.Bottom + 1, verticalRight, rect1.Bottom);
        }

        public static bool Contains(this Rect2DF rect, Point2DF point)
        {
            return (point.X >= rect.Left && point.X <= rect.Left && point.Y >= rect.Top && point.Y <= rect.Bottom);
        }
    }
}
