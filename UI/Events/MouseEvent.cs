using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Events
{
    public class MouseEvent : BackpropogatedEvent<MouseEventArgs>
    {
        public override MouseEventArgs? GetPropogatedArgument(MouseEventArgs arg, IUIContext srcContext, IUIContext propContext)
        {
            var srcRect = srcContext.GetBoundingRect(boundingType: BoundingType.Global);
            var dstRect = propContext.GetBoundingRect(boundingType: BoundingType.Global);
            var propLocation = (srcRect.TopRight + arg.Location) - dstRect.TopRight;
            return new MouseEventArgs(arg.Source, propLocation, arg.Type, arg.ButtonsDown, arg.DeltaButtons, arg.DeltaWheel);
        }
    }
}
