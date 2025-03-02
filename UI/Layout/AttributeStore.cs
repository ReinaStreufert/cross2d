using Cross.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public partial class AttributeStore : IAttributeProvider
    {
        public AttributeStore()
        {

        }

        private ConcurrentDictionary<Type, TypeStore> TypeStores = new ConcurrentDictionary<Type, TypeStore>();
        private TwoPriorityLock FreezeLock = new TwoPriorityLock();
        
        private ConcurrentDictionary<RootKey<T>, Attribute<T>> GetAttributeStore<T>()
        {
            var type = typeof(T);
            if (TypeStores.TryGetValue(type, out var typeStore))
                return typeStore.GetDict<T>();
            else
            {
                var newStore = new TypeStore<T>();
                return TypeStores
                    .GetOrAdd(type, newStore)
                    .GetDict<T>();
            }
        }

        public IAttributeContext CreateContext(IComponentTreeNode component)
        {
            return new MutableAttributeContext(this, component);
        }

        public IImmutableAttributeContext CreateImmutableContext(IComponentTreeNode component)
        {
            return new ImmutableAttributeContext(this, component);
        }

        public IDependencyCollectorAttrContext CreateDependencyCollectorContext(IComponentTreeNode component)
        {
            return new DependencyCollectorContext(this, component);
        }

        public async Task FreezeWhenSafeAsync()
        {
            await FreezeLock.AcquireSoleAsync();
        }

        public void Unfreeze()
        {
            FreezeLock.ReleaseSole();
        }

        public async Task InvokeFrozenWhenSafeAsync(Action callback)
        {
            await FreezeLock.AcquireSoleAsync();
            callback();
            FreezeLock.ReleaseSole();
        }

        public async Task<T> InvokeFrozenWhenSafeAsync<T>(Func<T> callback)
        {
            await FreezeLock.AcquireSoleAsync();
            var result = callback();
            FreezeLock.ReleaseSole();
            return result;
        }

        public async Task<T> InvokeFrozenWhenSafeAsync<T>(Func<Task<T>> asyncCallback)
        {
            await FreezeLock.AcquireSoleAsync();
            var result = await asyncCallback();
            FreezeLock.ReleaseSole();
            return result;
        }

        public async Task ReleaseComponentAttributeAsync(IComponent component)
        {
            await FreezeLock.AcquireSoleAsync();
        }

        public void ReleaseComponentAttributes(IComponent component)
        {
            
        }

        private abstract class TypeStore
        {
            public abstract ConcurrentDictionary<RootKey<TTry>, Attribute<TTry>> GetDict<TTry>();
            public abstract void ReleaseComponentAttributes(IComponent component);
        }

        private class TypeStore<T> : TypeStore
        {
            public ConcurrentDictionary<RootKey<T>, Attribute<T>> Store { get; } = new ConcurrentDictionary<RootKey<T>, Attribute<T>>(RootKeyComparer<T>.Instance);

            public override ConcurrentDictionary<RootKey<TTry>, Attribute<TTry>> GetDict<TTry>()
            {
                if (Store is ConcurrentDictionary<RootKey<TTry>, Attribute<TTry>> typedStore)
                    return typedStore;
                else
                    throw new InvalidCastException();
            }

            public override void ReleaseComponentAttributes(IComponent component)
            {
                for (; ;)
                {
                    int beginCount = Store.Count;
                    var componentKeys = Store
                        .Select(p => p.Key)
                        .Where(k => k.Node.Component == component)
                        .ToArray();
                    foreach (var key in componentKeys)
                        Store.TryRemove(key, out var _);
                    if (Store.Count == beginCount)
                        break;
                }
            }
        }

        private class Attribute<T>
        {
            public AttributeStore Provider { get; }
            public IComponentTreeNode Node { get; }
            public bool IsMacroLocked => _MacroKey != null;
            public T Value => _Unset ? throw new InvalidOperationException() : _Value!;
            public bool Unset => _Unset;

            public event Action? OnMutation;

            public Attribute(AttributeStore provider, IComponentTreeNode node)
            {
                Provider = provider;
                Node = node;
            }

            private T? _Value;
            private bool _Unset = true;
            private object _AttrWriteLock = new object();
            private object? _MacroKey;
            private long _LastUpdate = 0;

            public bool SetSharedAcquired(T value, long t)
            {
                lock (_AttrWriteLock)
                {
                    if (_LastUpdate > t || _MacroKey != null)
                        return false;
                    _Value = value;
                    _LastUpdate = t;
                    _Unset = false;
                }
                return true;
            }

            public bool SetSharedAcquired(IAttributeMacro<T> macro, long t, object macroKey)
            {
                lock (_AttrWriteLock)
                {
                    if (_MacroKey != null || t < _LastUpdate)
                        return false;
                    _MacroKey = macroKey;
                    _LastUpdate = t;
                }
                return true;
            }

            public async Task<bool> SetAsync(T value, bool sharedLockAcquired = false)
            {
                if (_MacroKey != null)
                    return false;
                var t = DateTime.Now.Ticks;
                bool result = sharedLockAcquired ? SetSharedAcquired(value, t) :
                    !await Provider.FreezeLock.LockedInvokeAsync(() => SetSharedAcquired(value, t));
                if (result)
                    OnMutation.SafeInvoke();
                return result;
            }

            public async Task<bool> SetMacroAsync(IAttributeMacro<T> macro, bool sharedLockAcquired = false)
            {
                if (_MacroKey != null)
                    return false;
                var t = DateTime.Now.Ticks;
                object macroKey = new object();
                bool result = sharedLockAcquired ? SetSharedAcquired(macro, t, macroKey) :
                    await Provider.FreezeLock.LockedInvokeAsync(() => SetSharedAcquired(macro, t, macroKey));
                await UpdateValueFromMacroAsync(macro, macroKey, null);
                return true;
            }

            public async Task<bool> ReleaseMacroAsync()
            {
                var t = DateTime.Now.Ticks;
                if (_MacroKey == null)
                    return true;
                return await Provider.FreezeLock.LockedInvokeAsync(() =>
                {
                    lock (_AttrWriteLock)
                    {
                        if (t < _LastUpdate)
                            return _MacroKey == null;
                        _MacroKey = null;
                        _LastUpdate = t;
                        return true;
                    }
                });
            }

            private async Task UpdateValueFromMacroAsync(IAttributeMacro<T> macro, object key, IDependencyCollectorAttrContext? dependencyContext)
            {
                dependencyContext?.ReleaseDependencies();
                if (_MacroKey != key)
                    return;
                var t = DateTime.Now.Ticks;
                // do not reuse the context, to prevent collection collisions between coinciding macro invalidations
                dependencyContext = new DependencyCollectorContext(Provider, Node);
                var macroResult = await macro.GetValueAsync(dependencyContext);
                await Provider.FreezeLock.LockedInvokeAsync(() =>
                {
                    lock (_AttrWriteLock)
                    {
                        if (_MacroKey != key || t < _LastUpdate)
                        {
                            dependencyContext.ReleaseDependencies();
                            return;
                        }
                        _LastUpdate = t;
                        _Value = macroResult;
                        _Unset = false;
                        dependencyContext.OnDependencyMutated += () => _ = UpdateValueFromMacroAsync(macro, key, dependencyContext);
                    }
                });
                OnMutation.SafeInvoke();
            }
        }

        private class RootKey<T>
        {
            public Key<T> Key { get; }
            public IComponentTreeNode Node { get; }

            public RootKey(Key<T> key, IComponentTreeNode node)
            {
                Key = key;
                Node = node;
            }
        }

        private class RootKeyComparer<T> : IEqualityComparer<RootKey<T>>
        {
            public static RootKeyComparer<T> Instance { get; } = new RootKeyComparer<T>();

            public bool Equals(RootKey<T>? x, RootKey<T>? y)
            {
                if (x == null || y == null)
                    return x == null && y == null;
                else return x.Node.Component == y.Node.Component && x.Node.Component == y.Node.Component;
            } 

            public int GetHashCode([DisallowNull] RootKey<T> obj)
            {
                return HashCode.Combine(obj.Node.Component, obj.Key);
            }
        }
    }
}
