using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Graphics
{
    public interface IRenderContext<TRenderable> where TRenderable : IRenderable
    {
        public void PushRenderTarget(TRenderable target);
        public void PopRenderTarget();
        public void PushClipRectangle(Rect2DF rect);
        public void PopClipRectangle();
    }

    public interface IRenderDevice<TRenderable> : IRenderContext<TRenderable> where TRenderable : class, IRenderable
    {
        public TRenderable NewRenderBuffer(Size2DF size);
        public void DrawGraphic(IComponentGraphic graphic, IImmutableAttributeContext attributeContext, TRenderable? inputGraphicBuffer, Rect2DF clipRectangle);
    }

    public interface IRenderContext<TRenderable, TBitmap, TBrush> : IRenderContext<TRenderable> where TRenderable : IRenderable where TBitmap : class, IBitmap where TBrush : class, IBrush
    {
        public TBitmap NewBitmap(Size2DF size);
        public TBrush NewSolidColorBrush(ColorRGBA color);
        public void FillRect(Rect2DF rect, TBrush brush);
        public void Draw(TRenderable image, Point2DF dstTopLeft);
        public void Draw(TRenderable image, Rect2DF srcRect, Point2DF dstTopLeft);
        public void Draw(TBitmap bmpImage, Point2DF dstTopLeft, float opacity = 1f);
        public void Draw(TBitmap bmpImage, Rect2DF srcRect, Point2DF dstTopLeft, float opacity = 1f);
        public void Draw(TBitmap bmpImage, Rect2DF srcRect, Rect2DF dstRect, float opacity = 1f);
    }

    public interface IBrush : IDisposable
    {
        BrushType Type { get; }
    }

    public interface IRenderable : IDisposable
    {
        public Size2DF Size { get; }
    }

    public interface IBitmap : IRenderable
    {

    }

    public enum BrushType
    {
        SolidColor
    }
}
