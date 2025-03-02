using Cross.Threading;
using Cross.UI.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public partial class EventDispatcher<TNode> : IEventDispatchProvider<TNode>
    {
        public IUIContextProvider<TNode> UIContextProvider { get; }

        public EventDispatcher(IUIContextProvider<TNode> uiContextProvider)
        {
            UIContextProvider = uiContextProvider;
        }

        private ConcurrentDictionary<Type, EventTypeStore> _EventTypes = new ConcurrentDictionary<Type, EventTypeStore>();

        public IDispatcherContext CreateContext(IComponentTreeNode<TNode> node)
        {
            return new DispatcherContext(this, node);
        }

        public IEventBindingContext CreateBindingOnlyContext(IComponentTreeNode<TNode> node)
        {
            return new DispatcherContext(this, node);
        }

        public void RegisterEventType<TEventType, TEventArg>(TEventType eventType) where TEventType : IEventType<TEventArg> where TEventArg : IEventArgument
        {
            var registration = new EventTypeRegistration<TEventType, TEventArg>(eventType);
            var store = new EventTypeStore<TEventType, TEventArg>(registration);
            _EventTypes.TryAdd(typeof(TEventType), store);
        }

        public void ReleaseComponentHandlers(IComponent component)
        {
            for (; ;)
            {
                int beginCount = _EventTypes.Count;
                foreach (var eventType in _EventTypes.Select(e => e.Value))
                    eventType.UnbindComponentHandlers(component);
                if (beginCount == _EventTypes.Count)
                    break;
            }
        }

        private abstract class EventTypeStore
        {
            public abstract EventTypeRegistration<TEventTypeTry, TArgTry> GetRegistration<TEventTypeTry, TArgTry>() where TArgTry : IEventArgument where TEventTypeTry : IEventType<TArgTry>;
            public abstract void UnbindComponentHandlers(IComponent component);
        }

        private class EventTypeStore<TEventType, TArg> : EventTypeStore where TArg : IEventArgument where TEventType : IEventType<TArg>
        {
            public EventTypeStore(EventTypeRegistration<TEventType, TArg> registration)
            {
                _Registration = registration;
            }

            private EventTypeRegistration<TEventType, TArg> _Registration;

            

            public override EventTypeRegistration<TEventTypeTry, TArgTry> GetRegistration<TEventTypeTry, TArgTry>()
            {
                if (_Registration is EventTypeRegistration<TEventTypeTry, TArgTry> typedRegistration)
                    return typedRegistration;
                else
                    throw new InvalidCastException();
            }

            public override void UnbindComponentHandlers(IComponent component)
            {
                var eventStore = _Registration.EventStore;
                for (; ;)
                {
                    int beginCount = eventStore.Count;
                    foreach (var dispatchList in eventStore.Select(p => p.Value))
                        dispatchList.UnbindComponentHandlers(component);
                    if (beginCount == eventStore.Count)
                        break;
                }
            }
        }

        private struct EventTypeRegistration<TEventType, TArg> where TArg : IEventArgument where TEventType : IEventType<TArg>
        {
            public ConcurrentDictionary<Key<TEventType>, EventDispatchList<TEventType, TArg>> EventStore;
            public TEventType EventType;

            public EventTypeRegistration(TEventType eventType)
            {
                EventStore = new ConcurrentDictionary<Key<TEventType>, EventDispatchList<TEventType, TArg>>();
                EventType = eventType;
            }
        }

        private class EventDispatchList<TEventType, TArg> where TArg : IEventArgument where TEventType : IEventType<TArg>
        {
            public Key<TEventType> Key { get; }
            public IUIContextProvider<TNode> UIContextProvider { get; }
            public TEventType EventType { get; }

            public EventDispatchList(Key<TEventType> key, IUIContextProvider<TNode> uiContextProvider, TEventType eventType)
            {
                Key = key;
                UIContextProvider = uiContextProvider;
                EventType = eventType;
            }

            private ConcurrentDictionary<IComponent, ComponentDispatchList<TArg>> _ComponentDispatchers = new ConcurrentDictionary<IComponent, ComponentDispatchList<TArg>>();

            public IEventBinding Bind(IComponentTreeNode<TNode> node, ComponentEventAsyncCallback<TArg> handler)
            {
                var dispatchList = _ComponentDispatchers.GetOrAdd(node.Component, (c) => new ComponentDispatchList<TArg>(node, UIContextProvider));
                return dispatchList.Bind(handler);
            }

            public void UnbindComponentHandlers(IComponent component)
            {
                _ComponentDispatchers.TryRemove(component, out var _);
            }

            public async Task DispatchAsync(IComponentTreeNode<TNode> origin, TArg arg)
            {
                var eventMode = EventType.Mode;
                if (eventMode == EventPropogationMode.Broadcast)
                {
                    await Task.WhenAll(_ComponentDispatchers.Values.Select(l => l.DispatchAsync(arg)));
                    return;
                }
                if (_ComponentDispatchers.TryGetValue(origin.Component, out var dispatchList))
                await dispatchList.DispatchAsync(arg);
                if (eventMode == EventPropogationMode.ChildToParent && origin.Parent != null)
                    await PropogateAsync(origin, origin.Parent, arg);
                else if (eventMode == EventPropogationMode.ParentToChild)
                    await Task.WhenAll(origin.Children.Select(dst => PropogateAsync(origin, dst, arg)));
            }

            public async Task PropogateAsync(IComponentTreeNode<TNode> src, IComponentTreeNode<TNode> dst, TArg srcArg)
            {
                var srcContext = UIContextProvider.GetUIContext(src);
                var dstContext = UIContextProvider.GetUIContext(dst);
                var propogatedArg = EventType.GetPropogatedArgument(srcArg, srcContext, dstContext);
                await DispatchAsync(dst, propogatedArg ?? srcArg);
            }
        }

        private class ComponentDispatchList<TArg> where TArg : IEventArgument
        {
            public IComponentTreeNode<TNode> Node { get; }
            public IUIContextProvider<TNode> UIContextProvider { get; }

            public ComponentDispatchList(IComponentTreeNode<TNode> node, IUIContextProvider<TNode> uiContextProvider)
            {
                Node = node;
                UIContextProvider = uiContextProvider;
            }

            private SpinList<ComponentEventAsyncCallback<TArg>> _Handlers = new SpinList<ComponentEventAsyncCallback<TArg>>();

            public IEventBinding Bind(ComponentEventAsyncCallback<TArg> callback)
            {
                return new EventBinding<TArg>(_Handlers.Add(callback), _Handlers, callback);
            }

            public async Task DispatchAsync(TArg arg)
            {
                await Task.WhenAll(InvokeAll(arg));
            }

            private IEnumerable<Task> InvokeAll(TArg arg)
            {
                foreach (var handler in _Handlers)
                {
                    var context = UIContextProvider.GetUIContext(Node);
                    yield return handler(context, arg);
                }
            }
        }

        private class EventBinding<TArg> : IEventBinding where TArg : IEventArgument
        {
            public EventBinding(int handlerId, SpinList<ComponentEventAsyncCallback<TArg>> handlerList, ComponentEventAsyncCallback<TArg> handler)
            {
                _HandlerId = handlerId;
                _Handler = handler;
                _HandlerList = handlerList;
            }

            private int _HandlerId;
            private ComponentEventAsyncCallback<TArg> _Handler;
            private SpinList<ComponentEventAsyncCallback<TArg>> _HandlerList;

            public void Unbind()
            {
                _HandlerList.Remove(_HandlerId, _Handler);
            }
        }
    }
}
