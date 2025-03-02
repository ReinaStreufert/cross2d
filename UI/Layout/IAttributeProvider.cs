using Cross.Threading;
using Cross.UI.Events;
using Cross.UI.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public interface IImmutableAttributeContext
    {
        public T GetAttribute<T>(Key<T> key);
        public T GetAttribute<T>(IComponentTreeNode descendant, Key<T> key);
        public bool TryGetAttribute<T>(Key<T> key, out T? val);
        public bool TryGetAttribute<T>(IComponentTreeNode descendant, Key<T> key, out T? val);
        public bool HasAttribute<T>(Key<T> key);
        public bool HasAttribute<T>(IComponentTreeNode descendant, Key<T> key);
    }

    public interface IDependencyCollectorAttrContext : IImmutableAttributeContext
    {
        public event Action? OnDependencyMutated;
        public void ReleaseDependencies();
    }

    public interface IAttributeContext : IImmutableAttributeContext
    {
        public Task BeginContiguousOperationAsync();
        public Task<bool> SetAttributeAsync<T>(Key<T> key, T value);
        public Task<bool> SetAttributeAsync<T>(IComponentTreeNode descendant, Key<T> key, T value);
        public Task<bool> SetAttributeMacroAsync<T>(Key<T> key, IAttributeMacro<T> macro);
        public Task<bool> SetAttributeMacroAsync<T>(IComponentTreeNode descendant, Key<T> key, IAttributeMacro<T> macro);
        public bool IsMacroLocked<T>(Key<T> key);
        public bool IsMacroLocked<T>(IComponentTreeNode descendant, Key<T> key);
        public Task<bool> ReleaseMacroAsync<T>(Key<T> key);
        public Task<bool> ReleaseMacroAsync<T>(IComponentTreeNode descendant, Key<T> key);
        public void EndContiguousOperation();
        public Task SetAttributesAsync(IEnumerable<IAttributeProvider.AttributeValuePair> pairs);
    }

    public interface IAttributeProvider
    {
        public IAttributeContext CreateContext(IComponentTreeNode component);
        public IImmutableAttributeContext CreateImmutableContext(IComponentTreeNode component);
        public IDependencyCollectorAttrContext CreateDependencyCollectorContext(IComponentTreeNode component);
        public Task FreezeWhenSafeAsync();
        public void Unfreeze();
        public Task InvokeFrozenWhenSafeAsync(Action callback);
        public Task<T> InvokeFrozenWhenSafeAsync<T>(Func<T> callback);
        public Task<T> InvokeFrozenWhenSafeAsync<T>(Func<Task<T>> asyncCallback);
        public void ReleaseComponentAttributes(IComponent component);

        protected class AttributeValuePair<T> : AttributeWriter
        {
            public Key<T> Key { get; }
            public T Value { get;}
            public IComponentTreeNode? Node { get; }

            public AttributeValuePair(IComponentTreeNode? node, Key<T> key, T value)
            {
                Key = key;
                Value = value;
                Node = node;
            }

            public override Task? Write(IAttributePairDestination context)
            {
                context.Set(Node, Key, Value);
                return null;
            }
        }

        protected class AttributeMacroPair<T> : AttributeWriter
        {
            public Key<T> Key { get; }
            public IAttributeMacro<T> Macro { get; }
            public IComponentTreeNode? Descendant { get; }

            public AttributeMacroPair(IComponentTreeNode? descendant, Key<T> key, IAttributeMacro<T> macro)
            {
                Key = key;
                Macro = macro;
                Descendant = descendant;
            }

            public override async Task Write(IAttributePairDestination context)
            {
                await context.SetMacroAsync(Descendant, Key, Macro);
            }
        }

        protected interface IAttributePairDestination
        {
            public void Set<T>(IComponentTreeNode? descendant, Key<T> key, T value);
            public Task SetMacroAsync<T>(IComponentTreeNode? descendant, Key<T> key, IAttributeMacro<T> macro);
        }

        protected abstract class AttributeWriter : AttributeValuePair
        {
            public abstract Task? Write(IAttributePairDestination context);
        }

        public abstract class AttributeValuePair
        {
            public static AttributeValuePair Create<T>(Key<T> key, T value) => new AttributeValuePair<T>(null, key, value);
            public static AttributeValuePair Create<T>(Key<T> key, IAttributeMacro<T> macro) => new AttributeMacroPair<T>(null, key, macro);
            public static AttributeValuePair Create<T>(IComponentTreeNode component, Key<T> key, T value) => new AttributeValuePair<T>(component, key, value);
            public static AttributeValuePair Create<T>(IComponentTreeNode component, Key<T> key, IAttributeMacro<T> macro) => new AttributeMacroPair<T>(component, key, macro);
        }
    }

    public interface IAttributeMacro<T>
    {
        public Task<T> GetValueAsync(IImmutableAttributeContext context);
    }
}
