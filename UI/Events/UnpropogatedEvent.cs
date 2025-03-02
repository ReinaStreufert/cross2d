using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cross.UI.Layout;

namespace Cross.UI.Events
{
    public class UnpropogatedEvent<TArg> : IEventType<TArg> where TArg : IEventArgument
    {
        public EventPropogationMode Mode => EventPropogationMode.Unpropogated;

        public TArg? GetPropogatedArgument(TArg arg, IUIContext srcContext, IUIContext propContext)
        {
            return default;
        }
    }
}
