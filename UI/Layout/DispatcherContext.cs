using Cross.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public partial class EventDispatcher<TNode>
    {
        private class DispatcherContext : EventBindingContext, IDispatcherContext
        {
            public DispatcherContext(EventDispatcher<TNode> dispatcher, IComponentTreeNode<TNode> origin) : base(dispatcher, origin)
            {
            }

            public async Task DispatchEventAsync<TEventType, TEventArg>(Key<TEventType> eventKey, TEventArg arg) where TEventType : IEventType<TEventArg> where TEventArg : IEventArgument
            {
                var e = FindEvent<TEventType, TEventArg>(eventKey);
                await e.DispatchAsync(_Origin, arg);
            }
        }
    }
    
}
