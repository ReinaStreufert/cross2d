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
        private class EventBindingContext : IEventBindingContext
        {
            public EventBindingContext(EventDispatcher<TNode> dispatcher, IComponentTreeNode<TNode> origin)
            {
                _Dispatcher = dispatcher;
                _Origin = origin;
            }

            protected EventDispatcher<TNode> _Dispatcher;
            protected IComponentTreeNode<TNode> _Origin;

            public void RegisterEventType<TEventType, TEventArg>(TEventType eventType) where TEventType : IEventType<TEventArg> where TEventArg : IEventArgument
            {
                _Dispatcher.RegisterEventType<TEventType, TEventArg>(eventType);
            }

            public IEventBinding SubscribeEvent<TEventType, TEventArg>(Key<TEventType> eventKey, ComponentEventAsyncCallback<TEventArg> asyncCallback) where TEventArg : IEventArgument where TEventType : IEventType<TEventArg>
            {
                var e = FindEvent<TEventType, TEventArg>(eventKey);
                return e.Bind(_Origin, asyncCallback);
            }

            protected EventDispatchList<TEventType, TEventArg> FindEvent<TEventType, TEventArg>(Key<TEventType> key) where TEventType : IEventType<TEventArg> where TEventArg : IEventArgument
            {
                if (!_Dispatcher._EventTypes.TryGetValue(typeof(TEventType), out var typeStore))
                    throw new InvalidOperationException("The event type is not registered");
                var typeRegistration = typeStore.GetRegistration<TEventType, TEventArg>();
                return typeRegistration.EventStore.GetOrAdd(key, (k) => new EventDispatchList<TEventType, TEventArg>(key, _Dispatcher.UIContextProvider, typeRegistration.EventType));
            }
        }
    }
}
