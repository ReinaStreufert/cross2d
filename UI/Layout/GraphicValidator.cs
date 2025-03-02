using Cross.Threading;
using Cross.UI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public partial class ComponentTree<TNodeResource, TRenderTarget>
    {
        private class GraphicValidation
        {
            public ComponentTree<TNodeResource, TRenderTarget> Processor { get; }
            public LayoutValidator Layout { get; }
            public int Index { get; }
            public IComponentGraphic Graphic { get; }
            public IDependencyCollectorAttrContext GraphicContext { get; }
            public IDependencyCollectorAttrContext OverflowRectContext { get; }
            public TRenderTarget? Buffer { get; set; }
            public Rect2DF LastOverflowRect { get; set; }
            public long LastInvalidated => _LastInvalidated;

            public GraphicValidation(LayoutValidator layout, int index, IComponentGraphic graphic, DateTime now)
            {
                Layout = layout;
                Index = index;
                Graphic = graphic;
                Processor = Layout.Processor;
                GraphicContext = Processor.AttributeProvider.CreateDependencyCollectorContext(layout);
                GraphicContext.OnDependencyMutated += DependencyCollectorCallback;
                OverflowRectContext = Processor.AttributeProvider.CreateDependencyCollectorContext(layout);
                OverflowRectContext.OnDependencyMutated += DependencyCollectorCallback;
                _LastInvalidated = now.Ticks;
                LastOverflowRect = layout.ContentRect;
            }

            private long _LastInvalidated;
            private long _LastOverflowValidation;

            private void SetThisInvalidated(DateTime time) => InterlockedMath.Max(ref _LastInvalidated, time.Ticks);
            public void SetInvalidated(DateTime time)
            {
                var graphicChain = Layout.GraphicChain;
                for (int i = Index; i < graphicChain.Length; i++)
                    graphicChain[i].SetThisInvalidated(time);
            }

            public void DoOverflowValidation(DateTime now)
            {
                var prevNode = Index == 0 ? null : Layout.GraphicChain[Index - 1];
                var prevNodeOverflowRect = prevNode?.LastOverflowRect ?? Layout.ContentRect;
                OverflowRectContext.ReleaseDependencies();
                LastOverflowRect = Graphic.GetOverflowRect(OverflowRectContext, prevNodeOverflowRect);
                _LastOverflowValidation = now.Ticks;
            }

            public void DoValidation(IRenderDevice<TRenderTarget> ctx, DirtyRectList dirtyRects)
            {
                if (_LastInvalidated > Processor.LastValidated)
                {
                    if (_LastOverflowValidation <= _LastInvalidated)
                        DoOverflowValidation(DateTime.Now);
                    if (Index == Layout.GraphicChain.Length - 1)
                        dirtyRects.Dirty(LastOverflowRect);
                    var prevNode = Index == 0 ? null : Layout.GraphicChain[Index - 1];
                    var buf = ValidateBuffer(ctx);
                    ctx.PushRenderTarget(buf);
                    var clip = new Rect2DF(0, 0, LastOverflowRect.Size);
                    ctx.PushClipRectangle(clip);
                    GraphicContext.ReleaseDependencies();
                    ctx.DrawGraphic(Graphic, GraphicContext, prevNode?.Buffer, clip);
                }
            }

            public void Release()
            {
                Buffer?.Dispose();
                GraphicContext.ReleaseDependencies();
                OverflowRectContext.ReleaseDependencies();
            }

            private TRenderTarget ValidateBuffer(IRenderDevice<TRenderTarget> dc)
            {
                var size = LastOverflowRect.Size;
                if (Buffer == null)
                    return CreateBuffer(dc, size);
                else
                {
                    var bufferSize = Buffer.Size;
                    if (bufferSize.Width < size.Width || bufferSize.Height < size.Height || bufferSize.Width > size.Width * 2f || bufferSize.Height > size.Height * 2f)
                        return CreateBuffer(dc, size);
                    else
                        return Buffer;
                }
            }

            private TRenderTarget CreateBuffer(IRenderDevice<TRenderTarget> ctx, Size2DF size)
            {
                if (Buffer != null)
                    Buffer.Dispose();
                var buf = ctx.NewRenderBuffer(size * 1.5f);
                Buffer = buf;
                return buf;
            }

            private void DependencyCollectorCallback()
            {
                SetInvalidated(DateTime.Now);
                Processor._Destination.Invalidate();
            }
        }
    }
}
