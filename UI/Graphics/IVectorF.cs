using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Graphics
{
    public interface IVectorF
    {
        int VecCount { get; }
        float this[int index] { get; }
    }

    public interface IVectorF<TSelf> : IVectorF where TSelf : IVectorF<TSelf>
    {
        TSelf CreateFrom(IVectorF eqLenVector);

        public abstract static bool operator ==(TSelf? a, IVectorF? b);
        public abstract static bool operator !=(TSelf? a, IVectorF? b);
    }

    public abstract class VectorF<TSelf> : IVectorF<TSelf> where TSelf : VectorF<TSelf>, new()
    {
        public float this[int index] => _VecArray[index];
        int IVectorF.VecCount => _VecCount;

        protected VectorF(int vecCount)
        {
            _VecCount = vecCount;
            _VecArray = new float[vecCount];
        }

        protected readonly float[] _VecArray;
        protected readonly int _VecCount;

        public TSelf CreateFrom(IVectorF eqLenVector)
        {
            var result = new TSelf();
            var count = result._VecCount;
            if (count != eqLenVector.VecCount)
                throw new ArgumentException($"{nameof(eqLenVector)} is not of equal length");
            for (int i = 0; i < count; i++)
                _VecArray[i] = eqLenVector[i];
            return result;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not VectorF<TSelf> vec) 
                return false;
            return this == vec;
        }

        public override int GetHashCode()
        {
            var count = _VecCount;
            var result = count.GetHashCode();
            for (int i = 0; i < count; i++)
                result ^= _VecArray[i].GetHashCode();
            return result;
        }

        static bool IVectorF<TSelf>.operator ==(TSelf? a, IVectorF? b)
        {
            var aNull = Equals(a, null);
            var bNull = Equals(b, null);
            if (aNull || bNull)
                return aNull == bNull;
            var count = a._VecCount;
            if (count != b.VecCount)
                return false;
            for (int i = 0; i < count; i++)
            {
                if (a._VecArray[i] != b[i])
                    return false;
            }
            return true;
        }

        static bool IVectorF<TSelf>.operator !=(TSelf? a, IVectorF? b)
        {
            var aNull = Equals(a, null);
            var bNull = Equals(b, null);
            if (aNull || bNull)
                return aNull != bNull;
            var count = a._VecCount;
            if (count != b.VecCount)
                return true;
            for (int i = 0; i < count; i++)
            {
                if (a._VecArray[i] == b[i])
                    return true;
            }
            return false;
        }

        public static TSelf operator +(VectorF<TSelf> a, IVectorF b) =>
            a.CreateFrom(new MatrixTransform(a, b, (i, x, y) => x + y));

        public static TSelf operator -(VectorF<TSelf> a, IVectorF b) =>
            a.CreateFrom(new MatrixTransform(a, b, (i, x, y) => x - y));

        public static TSelf operator *(VectorF<TSelf> a, IVectorF b) =>
            a.CreateFrom(new MatrixTransform(a, b, (i, x, y) => x * y));

        public static TSelf operator /(VectorF<TSelf> a, IVectorF b) =>
            a.CreateFrom(new MatrixTransform(a, b, (i, x, y) => x / y));

        public static TSelf operator +(VectorF<TSelf> a, float b) =>
            a.CreateFrom(new UniformTransform(a, (i, x) => x + b));

        public static TSelf operator -(VectorF<TSelf> a, float b) =>
            a.CreateFrom(new UniformTransform(a, (i, x) => x - b));

        public static TSelf operator *(VectorF<TSelf> a, float b) =>
            a.CreateFrom(new UniformTransform(a, (i, x) => x * b));

        public static TSelf operator /(VectorF<TSelf> a, float b) =>
            a.CreateFrom(new UniformTransform(a, (i, x) => x / b));
    }

    public class Vec1DF : VectorF<Vec1DF>
    {
        public float Value => _VecArray[0];

        public Vec1DF() : base(1)
        {
        }

        public Vec1DF(float value) : base(1)
        {
            _VecArray[0] = value;
        }
    }

    public class Point2DF : VectorF<Point2DF>
    {
        public float X => _VecArray[0];
        public float Y => _VecArray[1];

        public Point2DF() : base(2)
        {
        }

        public Point2DF(float x, float y) : base(2)
        {
            _VecArray[0] = x;
            _VecArray[1] = y;
        }
    }

    public class Size2DF : VectorF<Size2DF>
    {
        public float Width => _VecArray[0];
        public float Height => _VecArray[1];

        public Size2DF() : base(2)
        {
        }

        public Size2DF(float width, float height) : base(2)
        {
            _VecArray[0] = width;
            _VecArray[1] = height;
        }

        public Size2DF SubtractPadding(Padding2DF padding) => new Size2DF(
            Width - padding.Left - padding.Right,
            Height - padding.Top - padding.Bottom);
        public Size2DF AddMargin(Padding2DF margin) => new Size2DF(
            Width + margin.Left + margin.Right,
            Height + margin.Top + margin.Bottom);
    }

    public class Rect2DF : VectorF<Rect2DF>
    {
        public float Left => _VecArray[0];
        public float Top => _VecArray[1];
        public float Right => _VecArray[2];
        public float Bottom => _VecArray[3];
        public float Width => Right - Left + 1;
        public float Height => Top - Bottom + 1;
        public Size2DF Size => new Size2DF(Width, Height);
        public Point2DF TopLeft => new Point2DF(Left, Top);
        public Point2DF TopRight => new Point2DF(Right, Top);
        public Point2DF BottomLeft => new Point2DF(Left, Bottom);
        public Point2DF BottomRight => new Point2DF(Right, Bottom);
        public Point2DF Center => TopLeft + (Size / 2);
        
        public Rect2DF() : base(4)
        {
        }

        public Rect2DF(float left, float top, float right, float bottom) : base(4)
        {
            _VecArray[0] = left;
            _VecArray[1] = top;
            _VecArray[2] = right;
            _VecArray[3] = bottom;
        }

        public Rect2DF(Point2DF topLeft, Point2DF bottomRight) : base(4)
        {
            _VecArray[0] = topLeft.X;
            _VecArray[1] = topLeft.Y;
            _VecArray[2] = bottomRight.X;
            _VecArray[3] = bottomRight.Y;
        }

        public Rect2DF(Point2DF topLeft, Size2DF size) : this(topLeft, topLeft + size)
        {
            
        }

        public Rect2DF(Point2DF topLeft, float width, float height) : this(topLeft, new Size2DF(width, height))
        {

        }

        public Rect2DF(float left, float top, Size2DF size) : this(new Point2DF(left, top), size)
        {

        }

        public Rect2DF SubtractPadding(Padding2DF padding)
        {
            return new Rect2DF(Left + padding.Left, Top + padding.Top, Right - padding.Right, Bottom - padding.Bottom);
        }

        public Rect2DF AddMargin(Padding2DF margin)
        {
            return new Rect2DF(Left - margin.Left, Top - margin.Top, Right + margin.Right, Bottom + margin.Bottom);
        }
    }

    public class Padding2DF : VectorF<Padding2DF>
    {
        public float Left => _VecArray[0];
        public float Top => _VecArray[1];
        public float Right => _VecArray[2];
        public float Bottom => _VecArray[3];
        public Size2DF MinOccupied => new Size2DF(Left + Right, Top + Bottom);

        public Padding2DF() : base(4)
        {
        }

        public Padding2DF(float left, float top, float right, float bottom) : this()
        {
            _VecArray[0] = left;
            _VecArray[1] = top;
            _VecArray[2] = right;
            _VecArray[3] = bottom;
        }
    }

    public class ColorRGBA : VectorF<ColorRGBA>
    {
        public static ColorRGBA Transparent { get; } = new ColorRGBA(0f, 0f, 0f, 0f);

        public float R => _VecArray[0];
        public float G => _VecArray[1];
        public float B => _VecArray[2];
        public float A => _VecArray[3];
        public ColorHSVA HSVA => new ColorHSVA(this);

        public ColorRGBA() : base(4) { }

        public ColorRGBA(float r, float g, float b, float a) : base(4)
        {
            _VecArray[0] = r;
            _VecArray[1] = g;
            _VecArray[2] = b;
            _VecArray[3] = a;
        }

        public ColorRGBA(float r, float g, float b) : this(r, g, b, 1f)
        {
        }

        public ColorRGBA(ColorHSVA color) : base(4)
        {
            _VecArray[3] = color.A;
            var h = color.H;
            var s = color.S;
            var v = color.V;
            // no saturation, we can return the value across the board (grayscale)
            if (s == 0)
            {
                _VecArray[0] = v;
                _VecArray[1] = v;
                _VecArray[2] = v;
                return;
            }

            // which chunk of the rainbow are we in?
            float sector = h / 60;

            // split across the decimal (ie 3.87 into 3 and 0.87)
            int i = (int)sector;
            float f = sector - i;

            float p = v * (1 - s);
            float q = v * (1 - s * f);
            float t = v * (1 - s * (1 - f));

            switch (i)
            {
                case 0:
                    _VecArray[0] = v;
                    _VecArray[1] = t;
                    _VecArray[2] = p;
                    break;

                case 1:
                    _VecArray[0] = q;
                    _VecArray[1] = v;
                    _VecArray[2] = p;
                    break;

                case 2:
                    _VecArray[0] = p;
                    _VecArray[1] = v;
                    _VecArray[2] = t;
                    break;

                case 3:
                    _VecArray[0] = p;
                    _VecArray[1] = q;
                    _VecArray[2] = v;
                    break;

                case 4:
                    _VecArray[0] = t;
                    _VecArray[1] = p;
                    _VecArray[2] = v;
                    break;

                default:
                    _VecArray[0] = v;
                    _VecArray[1] = p;
                    _VecArray[2] = q;
                    break;
            }
        }
    }

    public class ColorHSVA : VectorF<ColorHSVA>
    {
        public float H => _VecArray[0];
        public float S => _VecArray[1];
        public float V => _VecArray[2];
        public float A => _VecArray[3];
        public ColorRGBA RGBA => new ColorRGBA(this);

        public ColorHSVA() : base(4) { }

        public ColorHSVA(float h, float s, float v, float a) : base(4)
        {
            _VecArray[0] = h;
            _VecArray[1] = s;
            _VecArray[2] = v;
            _VecArray[3] = a;
        }

        public ColorHSVA(ColorRGBA color) : base(4)
        {
            _VecArray[3] = color.A;
            float h, s, v;
            float min = Math.Min(Math.Min(color.R, color.G), color.B);
            float max = Math.Max(Math.Max(color.R, color.G), color.B);
            float delta = max - min;

            // value is our max color
            v = max;
            // saturation is percent of max
            if (max > 0)
                s = delta / max;
            else
            {
                // all colors are zero, no saturation and hue is undefined
                s = 0;
                h = -1;
                _VecArray[0] = h;
                _VecArray[1] = s;
                _VecArray[2] = v;
                return;
            }

            // grayscale image if min and max are the same
            if (min == max)
            {
                v = max;
                s = 0;
                h = -1;
                _VecArray[0] = h;
                _VecArray[1] = s;
                _VecArray[2] = v;
                return;
            }

            // hue depends which color is max (this creates a rainbow effect)
            if (color.R == max)
                h = (color.G - color.B) / delta;            // between yellow & magenta
            else if (color.G == max)
                h = 2 + (color.B - color.R) / delta;        // between cyan & yellow
            else
                h = 4 + (color.R - color.G) / delta;        // between magenta & cyan

            // turn hue into 0-360 degrees
            h *= 60;
            if (h < 0)
                h += 360;
            _VecArray[0] = h;
            _VecArray[1] = s;
            _VecArray[2] = v;
        }

        public ColorHSVA(float h, float s, float v) : this(h, s, v, 1f) { }
    }
}
