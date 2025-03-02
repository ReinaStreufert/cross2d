using Cross.Threading;
using Cross.UI.Events;
using Cross.UI.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public class UIContext : IUIContext
    {
        public UIContext(IDispatcherContext dispatcherContext, IAttributeContext attributeContext, IComponentTreeNode componentNode, ConcurrentDictionary<Key<IComponent>, IComponentTreeNode> documentDict, IValidated uiValidator)
        {
            _DispatcherContext = dispatcherContext;
            _AttributeContext = attributeContext;
            _ComponentNode = componentNode;
            _DocumentDict = documentDict;
            _UIValidator = uiValidator;
        }

        private IDispatcherContext _DispatcherContext;
        private IAttributeContext _AttributeContext;
        private IComponentTreeNode _ComponentNode;
        private IValidated _UIValidator;
        private ConcurrentDictionary<Key<IComponent>, IComponentTreeNode> _DocumentDict;

        public Rect2DF GetBoundingRect(IComponentTreeNode? descendant = null, BoundingType boundingType = BoundingType.Client, bool includeOverflow = false)
        {
            var node = descendant ?? _ComponentNode;
            var result = includeOverflow ? node.OverflowRect : node.ContentRect;
            if (boundingType == BoundingType.Client)
                return _ComponentNode.ContentRect - result;
            else
                return result;
        }

        public IComponentTreeNode GetDescendantNodeByKey(Key<IComponent> key)
        {
            if (!_DocumentDict.TryGetValue(key, out var node))
                throw new ArgumentException($"No component identified by '{nameof(key)}' does not exist");
            if (!node.IsDescendant(_ComponentNode.Component))
                throw new ArgumentException($"No descendant identified by '{nameof(key)}' exists");
            return node;
        }

        public Task<DateTime> SyncLayoutUpdatesAsync()
        {
            return _UIValidator.Sync();
        }

        public Task<DateTime> SyncLayoutUpdatesAsync(DateTime minValidTo)
        {
            return _UIValidator.Sync(minValidTo);
        }

        public Task DispatchEventAsync<TEventType, TEventArg>(Key<TEventType> eventKey, TEventArg arg)
            where TEventType : IEventType<TEventArg>
            where TEventArg : IEventArgument => _DispatcherContext.DispatchEventAsync(eventKey, arg);
        public void RegisterEventType<TEventType, TEventArg>(TEventType eventType)
            where TEventType : IEventType<TEventArg>
            where TEventArg : IEventArgument => _DispatcherContext.RegisterEventType<TEventType, TEventArg>(eventType);
        public IEventBinding SubscribeEvent<TEventType, TEventArg>(Key<TEventType> eventKey, ComponentEventAsyncCallback<TEventArg> asyncCallback)
            where TEventType : IEventType<TEventArg>
            where TEventArg : IEventArgument => _DispatcherContext.SubscribeEvent(eventKey, asyncCallback);
        public Task BeginContiguousOperationAsync() => _AttributeContext.BeginContiguousOperationAsync();
        public void EndContiguousOperation() => _AttributeContext.EndContiguousOperation();
        public T GetAttribute<T>(Key<T> key) => _AttributeContext.GetAttribute(key);
        public T GetAttribute<T>(IComponentTreeNode descendant, Key<T> key) => _AttributeContext.GetAttribute(descendant, key);
        public bool HasAttribute<T>(Key<T> key) => _AttributeContext.HasAttribute(key);
        public bool HasAttribute<T>(IComponentTreeNode descendant, Key<T> key) => _AttributeContext.HasAttribute(descendant, key);
        public bool IsMacroLocked<T>(Key<T> key) => _AttributeContext.IsMacroLocked(key);
        public bool IsMacroLocked<T>(IComponentTreeNode descendant, Key<T> key) => _AttributeContext.IsMacroLocked(descendant, key);
        public Task<bool> ReleaseMacroAsync<T>(Key<T> key) => _AttributeContext.ReleaseMacroAsync(key);
        public Task<bool> ReleaseMacroAsync<T>(IComponentTreeNode descendant, Key<T> key) => _AttributeContext.ReleaseMacroAsync(descendant, key);
        public Task<bool> SetAttributeAsync<T>(Key<T> key, T value) => _AttributeContext.SetAttributeAsync(key, value);
        public Task<bool> SetAttributeAsync<T>(IComponentTreeNode descendant, Key<T> key, T value) => _AttributeContext.SetAttributeAsync(descendant, key, value);
        public Task<bool> SetAttributeMacroAsync<T>(Key<T> key, IAttributeMacro<T> macro) => _AttributeContext.SetAttributeMacroAsync(key, macro);
        public Task<bool> SetAttributeMacroAsync<T>(IComponentTreeNode descendant, Key<T> key, IAttributeMacro<T> macro) => _AttributeContext.SetAttributeMacroAsync(descendant, key, macro);
        public Task SetAttributesAsync(IEnumerable<IAttributeProvider.AttributeValuePair> pairs) => _AttributeContext.SetAttributesAsync(pairs);
        public bool TryGetAttribute<T>(Key<T> key, out T? val) => _AttributeContext.TryGetAttribute(key, out val);
        public bool TryGetAttribute<T>(IComponentTreeNode descendant, Key<T> key, out T? val) => _AttributeContext.TryGetAttribute(descendant, key, out val);
    }
}
