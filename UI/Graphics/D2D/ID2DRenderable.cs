using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Graphics.D2D
{
    public interface ID2DRenderable : IRenderable
    {
        SharpDX.Direct2D1.Image Image { get; }
    }

    public interface ID2DBitmap : IBitmap, ID2DRenderable
    {
        SharpDX.Direct2D1.Bitmap Bitmap { get; }
    }

    public interface ID2DBrush : IBrush
    {
        SharpDX.Direct2D1.Brush Brush { get; }
    }

    public class D2DBitmap : ID2DBitmap
    {
        public Size2DF Size => _Bitmap.Size.ToSize2DF();
        public SharpDX.Direct2D1.Bitmap Bitmap => _Bitmap;
        SharpDX.Direct2D1.Image ID2DRenderable.Image => _Bitmap;

        public D2DBitmap(Bitmap1 bitmap)
        {
            _Bitmap = bitmap;
        }

        private Bitmap1 _Bitmap;

        public void Dispose()
        {
            _Bitmap.Dispose();
        }
    }

    public class D2DSolidColorBrush : ID2DBrush
    {
        public SharpDX.Direct2D1.Brush Brush => _Brush;
        BrushType IBrush.Type => BrushType.SolidColor;

        public D2DSolidColorBrush(SolidColorBrush brush)
        {
            _Brush = brush;
        }

        private SolidColorBrush _Brush;

        public void Dispose()
        {
            _Brush.Dispose();
        }
    }
}
