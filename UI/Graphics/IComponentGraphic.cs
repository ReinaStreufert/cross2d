using Cross.UI.Layout;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Graphics
{
    public interface IComponentGraphic
    {
        public Padding2DF GetOverflow(IImmutableAttributeContext context, Size2DF inputSize);
        public void Draw<TRenderable, TBitmap, TBrush>(IImmutableAttributeContext attributes, IRenderContext<TRenderable, TBitmap, TBrush> ctx, TRenderable? inputGraphicBuffer, Rect2DF clipRectangle) where TRenderable : class, IRenderable where TBitmap : class, IBitmap where TBrush : class, IBrush;
    }
}
