using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Events
{
    public interface IEventType<TArg> where TArg : IEventArgument
    {
        public EventPropogationMode Mode { get; }
        public TArg? GetPropogatedArgument(TArg arg, IUIContext srcContext, IUIContext propContext);
    }

    public interface IEventArgument
    {
        public IComponent? Source { get; }
    }

    public interface IEventBinding
    {
        public void Unbind();
    }

    public interface IEventRegistry
    {
        public void RegisterEventType<TEventType, TEventArg>(TEventType eventType) where TEventType : IEventType<TEventArg> where TEventArg : IEventArgument;
    }

    public interface IEventBindingContext : IEventRegistry
    {
        public IEventBinding SubscribeEvent<TEventType, TEventArg>(Key<TEventType> eventKey, ComponentEventAsyncCallback<TEventArg> asyncCallback) where TEventType : IEventType<TEventArg> where TEventArg : IEventArgument;
    }

    public interface IDispatcherContext : IEventBindingContext
    {
        public Task DispatchEventAsync<TEventType, TEventArg>(Key<TEventType> eventKey, TEventArg arg) where TEventType : IEventType<TEventArg> where TEventArg : IEventArgument;
    }

    public interface IEventDispatchProvider<T> : IEventRegistry
    {
        public IDispatcherContext CreateContext(IComponentTreeNode<T> node);
        public IEventBindingContext CreateBindingOnlyContext(IComponentTreeNode<T> node);
        public void ReleaseComponentHandlers(IComponent component);
    }

    public enum EventPropogationMode
    {
        Broadcast,
        ChildToParent,
        ParentToChild,
        Unpropogated
    }
}
