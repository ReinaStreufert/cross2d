using Cross.Threading;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Graphics.D2D
{
    public class D2DCompositor : Validated, ICompositeDestination<ID2DRenderable>
    {
        public D2DCompositor(D2DWindowContext windowContext)
        {
            _DeviceContext = windowContext.DeviceContext;
            _BackBufferTarget = windowContext.BackBufferTarget;
            _SwapChain = windowContext.SwapChain;
            _PixelRatio = _DeviceContext.GetPixelRatio();
        }

        private float _PixelRatio;
        private DeviceContext _DeviceContext;
        private Bitmap1 _BackBufferTarget;
        private SwapChain1 _SwapChain;
        private ICompositionSource<ID2DRenderable>? _CompositionSource;

        public void SetCompositionSource(ICompositionSource<ID2DRenderable> compositionSource)
        {
            if (_CompositionSource != null)
                throw new InvalidOperationException("The composition source is already set");
            _CompositionSource = compositionSource;
        }

        protected override async Task<DateTime> ValidateAsync()
        {
            if (_CompositionSource == null)
                throw new InvalidOperationException("Composition source is unset");
            CompositionFrame<ID2DRenderable> composition;
            var dc = _DeviceContext;
            dc.BeginDraw();
            var renderCtx = new D2DRenderContext(dc);
            composition = await _CompositionSource.ComposeFrameAsync(renderCtx);
            var dirtyRegion = composition.Dirty;
            dc.Target = _BackBufferTarget;
            foreach (var composite in composition.Rectangles)
            {
                foreach (var intersectionRect in dirtyRegion.FindIntersections(composite.Rect))
                {
                    var srcRect = intersectionRect - composite.Rect;
                    var bmp = composite.Buffer;
                    dc.DrawImage(bmp.Image, intersectionRect.TopLeft.ToD2DVector2(), srcRect.ToD2DRectF(), InterpolationMode.Linear, CompositeMode.SourceOver);
                }
            }
            dc.EndDraw();
            var scrollRect = (composition.WindowRectangle * _PixelRatio)
                .ToD2DRectRound();
            var dirtyRects = dirtyRegion
                .Select(r2d => (r2d * _PixelRatio).ToD2DRectRound())
                .ToArray();
            var presentParameters = new PresentParameters()
            {
                ScrollRectangle = scrollRect,
                ScrollOffset = null,
                DirtyRectangles = dirtyRects
            };
            _SwapChain.Present(0, PresentFlags.None, presentParameters);
            return composition.ValidAt;
        }
    }
}
