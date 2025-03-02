using Cross.UI.Graphics;
using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Components
{
    public class BackgroundGraphic : IComponentGraphic
    {
        public Rect2DF GetOverflowRect(IImmutableAttributeContext context, Rect2DF inputRect)
        {
            return inputRect;
        }

        void IComponentGraphic.Draw<TRenderable, TBitmap, TBrush>(IImmutableAttributeContext attributes, IRenderContext<TRenderable, TBitmap, TBrush> ctx, TRenderable? inputGraphicBuffer, Rect2DF clipRectangle) where TRenderable : class
        {
            var bgColor = attributes.GetAttributeOrDefault(Attributes.BackgroundColor, ColorRGBA.Transparent);
            using (var brush = ctx.NewSolidColorBrush(bgColor))
                ctx.FillRect(clipRectangle, brush);
        }
    }
}
