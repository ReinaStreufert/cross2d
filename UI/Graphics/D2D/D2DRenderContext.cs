using Cross.UI.Layout;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Graphics.D2D
{
    public class D2DRenderContext : IRenderDevice<ID2DRenderable>, IRenderContext<ID2DRenderable, ID2DBitmap, ID2DBrush>
    {
        public D2DRenderContext(DeviceContext dc)
        {
            _DC = dc;
        }

        private DeviceContext _DC;
        private Stack<ID2DRenderable> _TargetStack = new Stack<ID2DRenderable>();

        void IRenderDevice<ID2DRenderable>.DrawGraphic(IComponentGraphic graphic, IImmutableAttributeContext attributeContext, ID2DRenderable? inputGraphicBuffer, Rect2DF clipRectangle)
        {
            graphic.Draw(attributeContext, this, inputGraphicBuffer, clipRectangle);
        }

        public void PopRenderTarget()
        {
            _TargetStack.Pop();
            _DC.Target = _TargetStack.Peek().Image;
        }

        public void PushRenderTarget(ID2DRenderable target)
        {
            _DC.Target = target.Image;
            _TargetStack.Push(target);
        }

        public void PushClipRectangle(Rect2DF rect)
        {
            _DC.PushAxisAlignedClip(rect.ToD2DRectF(), AntialiasMode.Aliased);
        }

        public void PopClipRectangle()
        {
            _DC.PopAxisAlignedClip();
        }

        public ID2DRenderable NewRenderBuffer(Size2DF size)
        {
            return NewBitmap(size);
        }

        public ID2DBitmap NewBitmap(Size2DF size)
        {
            var bitmap = new Bitmap1(_DC, size.ToD2DSizeRound());
            return new D2DBitmap(bitmap);
        }

        public ID2DBrush NewSolidColorBrush(ColorRGBA color)
        {
            var solidColorBrush = new SolidColorBrush(_DC, color.ToD2DColor4());
            return new D2DSolidColorBrush(solidColorBrush);
        }

        public void FillRect(Rect2DF rect, ID2DBrush brush)
        {
            _DC.FillRectangle(rect.ToD2DRectF(), brush.Brush);
        }

        public void Draw(ID2DRenderable image, Point2DF dstTopLeft)
        {
            _DC.DrawImage(image.Image, dstTopLeft.ToD2DVector2());
        }

        public void Draw(ID2DRenderable image, Rect2DF srcRect, Point2DF dstTopLeft)
        {
            _DC.DrawImage(
                image.Image,
                dstTopLeft.ToD2DVector2(),
                srcRect.ToD2DRectF(), 
                InterpolationMode.Linear, 
                CompositeMode.SourceOver);
        }

        public void Draw(ID2DBitmap bmpImage, Point2DF dstTopLeft, float opacity = 1f)
        {
            _DC.DrawBitmap(
                bmpImage.Bitmap, 
                new Rect2DF(dstTopLeft, bmpImage.Size).ToD2DRectF(), 
                opacity,
                BitmapInterpolationMode.Linear);
        }

        public void Draw(ID2DBitmap bmpImage, Rect2DF srcRect, Point2DF dstTopLeft, float opacity = 1f)
        {
            _DC.DrawBitmap(
                bmpImage.Bitmap,
                new Rect2DF(dstTopLeft, bmpImage.Size).ToD2DRectF(),
                opacity, 
                BitmapInterpolationMode.Linear,
                srcRect.ToD2DRectF());
        }

        public void Draw(ID2DBitmap bmpImage, Rect2DF srcRect, Rect2DF dstRect, float opacity = 1f)
        {
            _DC.DrawBitmap(
                bmpImage.Bitmap,
                dstRect.ToD2DRectF(),
                opacity,
                BitmapInterpolationMode.Linear,
                srcRect.ToD2DRectF());
        }
    }
}
