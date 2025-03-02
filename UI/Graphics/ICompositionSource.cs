using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Graphics
{
    public interface ICompositionSource<TRenderable> where TRenderable : class, IRenderable
    {
        public Task<CompositionFrame<TRenderable>> ComposeFrameAsync(IRenderDevice<TRenderable> ctx);
    }

    public class CompositionFrame<TRenderTarget> where TRenderTarget : class, IRenderable
    {
        public Rect2DF WindowRectangle { get; }
        public DateTime ValidAt { get; }
        public CompositeRect<TRenderTarget>[] Rectangles { get; }
        public DirtyRectList Dirty { get; }

        public CompositionFrame(Rect2DF windowRectangle, DateTime validAt, CompositeRect<TRenderTarget>[] rectangles, DirtyRectList dirty)
        {
            WindowRectangle = windowRectangle;
            Rectangles = rectangles;
            ValidAt = validAt;
            Dirty = dirty;
        }
    }

    public class CompositeRect<TRenderable> where TRenderable : class, IRenderable
    {
        public Rect2DF Rect { get; }
        public TRenderable Buffer { get; }

        internal CompositeRect(Rect2DF location, TRenderable buffer)
        {
            Rect = location;
            Buffer = buffer;
        }
    }
}
