using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Events
{
    public class BroadcastEvent<T> : IEventType<T> where T : BasicEventArgs
    {
        public EventPropogationMode Mode => EventPropogationMode.Broadcast;

        public T? GetPropogatedArgument(T arg, IUIContext srcContext, IUIContext propContext)
        {
            return arg;
        }
    }
}
