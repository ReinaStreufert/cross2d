using Cross.UI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public partial class ComponentTree<TNodeResource, TRenderTarget>
    {
        private class LayoutContext : ILayoutContext
        {
            public IEnumerable<ILayoutComponent> Elements => _Node.ChildList
                .Select(x => new LayoutComponent(x));

            public LayoutContext(LayoutNode node, IImmutableAttributeContext attrContext)
            {
                _Node = node;
                _AttrContext = attrContext;
            }

            private LayoutNode _Node;
            private IImmutableAttributeContext _AttrContext;

            public T GetAttribute<T>(Key<T> key) => _AttrContext.GetAttribute(key);
            public T GetAttribute<T>(IComponentTreeNode descendant, Key<T> key) => _AttrContext.GetAttribute(descendant, key);
            public bool HasAttribute<T>(Key<T> key) => _AttrContext.HasAttribute(key);
            public bool HasAttribute<T>(IComponentTreeNode descendant, Key<T> key) => _AttrContext.HasAttribute(descendant, key);
            public bool TryGetAttribute<T>(Key<T> key, out T? val) => _AttrContext.TryGetAttribute(key, out val);
            public bool TryGetAttribute<T>(IComponentTreeNode descendant, Key<T> key, out T? val) => _AttrContext.TryGetAttribute(descendant, key, out val);

            private class LayoutComponent : ILayoutComponent
            {
                public IComponentTreeNode ComponentNode => _Node;
                public LayoutSize SpatialSize => _Node.RelativeSizeValidator.Size;

                public LayoutComponent(LayoutNode node)
                {
                    _Node = node;
                }

                private LayoutNode _Node;
            }
        }
    }
}
