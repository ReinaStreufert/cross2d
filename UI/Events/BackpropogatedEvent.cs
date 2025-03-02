using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Events
{
    public class BackpropogatedEvent<TArg> : IEventType<TArg> where TArg : InterruptibleEventArgs
    {
        public EventPropogationMode Mode => EventPropogationMode.ChildToParent;

        public virtual TArg? GetPropogatedArgument(TArg arg, IUIContext srcContext, IUIContext propContext)
        {
            if (!arg.IsPropogated)
                return null;
            return arg;
        }
    }
}
