using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public partial class AttributeStore
    {
        private class ImmutableAttributeContext : IImmutableAttributeContext
        {
            public AttributeStore Provider { get; }
            public IComponentTreeNode Node { get; }

            public ImmutableAttributeContext(AttributeStore provider, IComponentTreeNode node)
            {
                Provider = provider;
                Node = node;
            }

            public T GetAttribute<T>(Key<T> key)
            {
                var attr = FindAttribute(RootKey(key));
                if (attr == null)
                    throw new ArgumentException($"The component does not have the attribute '{nameof(key)}'");
                return attr.Value;
            }

            public T GetAttribute<T>(IComponentTreeNode componentNode, Key<T> key)
            {
                var attr = FindAttribute(RootKey(key, componentNode));
                if (attr == null)
                    throw new ArgumentException($"'{nameof(componentNode)}' does not have the attribute '{nameof(key)}'");
                return attr.Value;
            }

            public bool TryGetAttribute<T>(Key<T> key, out T? val)
            {
                var attr = FindAttribute(RootKey(key));
                if (attr == null)
                {
                    val = default;
                    return false;
                }
                val = attr.Value;
                return true;
            }

            public bool TryGetAttribute<T>(IComponentTreeNode componentNode, Key<T> key, out T? val)
            {
                var attr = FindAttribute(RootKey(key, componentNode));
                if (attr == null)
                {
                    val = default;
                    return false;
                }
                val = attr.Value;
                return true;
            }

            public bool HasAttribute<T>(Key<T> key)
            {
                var store = Provider.GetAttributeStore<T>();
                return store.ContainsKey(RootKey(key));
            }

            public bool HasAttribute<T>(IComponentTreeNode component, Key<T> key)
            {
                var store = Provider.GetAttributeStore<T>();
                return store.ContainsKey(RootKey(key, component));
            }

            protected RootKey<T> RootKey<T>(Key<T> key, IComponentTreeNode? descendant = null)
            {
                if (descendant == null)
                    return new RootKey<T>(key, Node);
                if (!descendant.IsDescendant(Node.Component))
                    throw new ArgumentException($"'{nameof(descendant)}' is not a descendant of the component context");
                return new RootKey<T>(key, descendant);
            }

            protected virtual Attribute<T>? FindAttribute<T>(RootKey<T> key)
            {
                var store = Provider.GetAttributeStore<T>();
                if (store.TryGetValue(key, out var attr) && !attr.Unset)
                    return attr;
                else
                    return null;
            }

            protected virtual Attribute<T>? FindAttribute<T>(RootKey<T> key, out ConcurrentDictionary<RootKey<T>, Attribute<T>> store)
            {
                store = Provider.GetAttributeStore<T>();
                if (store.TryGetValue(key, out var attr) && !attr.Unset)
                    return attr;
                else
                    return null;
            }
        }
    }
}
