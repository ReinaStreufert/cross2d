using Cross.UI.Events;
using Cross.UI.Graphics;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public interface IUIContext : IDispatcherContext, IAttributeContext
    {
        public IComponentTreeNode GetDescendantNodeByKey(Key<IComponent> key);
        public Rect2DF GetBoundingRect(IComponentTreeNode? descendant = null, BoundingType boundingType = BoundingType.Client, bool includeOverflow = false);
        public Task<DateTime> SyncLayoutUpdatesAsync();
        public Task<DateTime> SyncLayoutUpdatesAsync(DateTime minValidTo);
    }

    public interface IUIContextProvider<T>
    {
        public IUIContext GetUIContext(IComponentTreeNode<T> node);
    }

    public enum BoundingType
    {
        Client,
        Global
    }
}
