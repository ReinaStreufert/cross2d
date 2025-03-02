using Cross.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public partial class AttributeStore
    {
        private class DependencyCollectorContext : ImmutableAttributeContext, IDependencyCollectorAttrContext
        {
            public event Action? OnDependencyMutated;

            public DependencyCollectorContext(AttributeStore provider, IComponentTreeNode node) : base(provider, node)
            {
                
            }

            private event Action? OnRelease;
            
            protected override Attribute<T>? FindAttribute<T>(RootKey<T> key)
            {
                var attr = base.FindAttribute(key, out var store);
                Action a = OnDependencyMutated.SafeInvoke;
                if (attr != null)
                {
                    attr.OnMutation += a;
                    OnRelease += () =>
                    {
                        attr.OnMutation -= a;
                    };
                } else
                {
                    var unsetAttribute = new Attribute<T>(Provider, key.Node);
                    unsetAttribute.OnMutation += a;
                    OnRelease += () =>
                    {
                        unsetAttribute.OnMutation -= a;
                    };
                    store.TryAdd(key, unsetAttribute);
                }
                return attr;
            }

            public void ReleaseDependencies()
            {
                OnRelease.SafeInvoke();
                OnRelease = null;
            }
        }
    }
}
