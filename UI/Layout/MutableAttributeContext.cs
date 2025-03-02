using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public partial class AttributeStore
    {
        private class MutableAttributeContext : ImmutableAttributeContext, IAttributeContext
        {
            public MutableAttributeContext(AttributeStore provider, IComponentTreeNode node) : base(provider, node)
            {
            }

            private bool _SharedPreAcquired = false;

            public async Task BeginContiguousOperationAsync()
            {
                if (_SharedPreAcquired)
                    throw new InvalidOperationException("End the last contiguous operation before beginning a new one");
                await Provider.FreezeLock.AcquireSharedAsync();
                _SharedPreAcquired = true;
            }

            public void EndContiguousOperation()
            {
                if (!_SharedPreAcquired)
                    throw new InvalidOperationException("No contiguous operation was started");
                Provider.FreezeLock.ReleaseShared();
                _SharedPreAcquired = false;
            }

            public bool IsMacroLocked<T>(Key<T> key)
            {
                var attr = FindAttribute(RootKey(key));
                if (attr == null)
                    return false;
                return attr.IsMacroLocked;
            }

            public bool IsMacroLocked<T>(IComponentTreeNode descendant, Key<T> key)
            {
                var attr = FindAttribute(RootKey(key));
                if (attr == null)
                    throw new ArgumentException($"{nameof(key)} is unset");
                return attr.IsMacroLocked;
            }

            public async Task<bool> ReleaseMacroAsync<T>(Key<T> key)
            {
                var attr = FindAttribute(RootKey(key));
                if (attr == null)
                    throw new ArgumentException($"{nameof(key)} is unset");
                return await attr.ReleaseMacroAsync();
            }

            public async Task<bool> ReleaseMacroAsync<T>(IComponentTreeNode descendant, Key<T> key)
            {
                var attr = FindAttribute(RootKey(key, descendant));
                if (attr == null)
                    throw new ArgumentException($"{nameof(key)} is unset");
                return await attr.ReleaseMacroAsync();
            }

            public async Task<bool> SetAttributeAsync<T>(Key<T> key, T value)
            {
                var attr = FindOrCreate(RootKey(key));
                return await attr.SetAsync(value, _SharedPreAcquired);
            }

            public async Task<bool> SetAttributeAsync<T>(IComponentTreeNode descendant, Key<T> key, T value)
            {
                var attr = FindOrCreate(RootKey(key, descendant));
                return await attr.SetAsync(value, _SharedPreAcquired);
            }

            public async Task<bool> SetAttributeMacroAsync<T>(Key<T> key, IAttributeMacro<T> macro)
            {
                var attr = FindOrCreate(RootKey(key));
                return await attr.SetMacroAsync(macro, _SharedPreAcquired);
            }

            public async Task<bool> SetAttributeMacroAsync<T>(IComponentTreeNode descendant, Key<T> key, IAttributeMacro<T> macro)
            {
                var attr = FindOrCreate(RootKey(key, descendant));
                return await attr.SetMacroAsync(macro, _SharedPreAcquired);
            }

            public async Task SetAttributesAsync(IEnumerable<IAttributeProvider.AttributeValuePair> pairs)
            {
                var dst = new AttributePairDestination(this);
                await Provider.FreezeLock.AcquireSharedAsync();
                await Task.WhenAll(pairs
                    .Select(p => ((IAttributeProvider.AttributeWriter)pairs).Write(dst))
                    .Where(t => t != null)!);
                Provider.FreezeLock.ReleaseShared();
            }

            protected Attribute<T> FindOrCreate<T>(RootKey<T> key)
            {
                var typeStore = Provider.GetAttributeStore<T>();
                return typeStore.GetOrAdd(key, (k) => new Attribute<T>(Provider, k.Node));
            }

            private class AttributePairDestination : IAttributeProvider.IAttributePairDestination
            {
                private MutableAttributeContext Context;

                public AttributePairDestination(MutableAttributeContext context)
                {
                    Context = context;
                }

                public void Set<T>(IComponentTreeNode? descendant, Key<T> key, T value)
                {
                    var attr = Context.FindOrCreate(Context.RootKey(key, descendant));
                    attr.SetSharedAcquired(value, DateTime.Now.Ticks);
                }

                public async Task SetMacroAsync<T>(IComponentTreeNode? descendant, Key<T> key, IAttributeMacro<T> macro)
                {
                    var attr = Context.FindOrCreate(Context.RootKey(key, descendant));
                    await attr.SetMacroAsync(macro);
                }
            }
        }
    }
}
