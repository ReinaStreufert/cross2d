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
        private class RelativeSizeValidator
        {
            public ComponentTree<TNodeResource, TRenderTarget> Tree { get; }
            public LayoutNode Node { get; }
            public LayoutSize Size { get; private set; }

            public RelativeSizeValidator(ComponentTree<TNodeResource, TRenderTarget> tree, LayoutNode node)
            {
                Tree = tree;
                Node = node;
                Size = new LayoutSize(
                    SpatialUnit<Size2DF>.Absolute(new Size2DF()),
                    SpatialUnit<Padding2DF>.Absolute(new Padding2DF()),
                    SpatialUnit<Padding2DF>.Absolute(new Padding2DF()));
                _AttrContext = tree.AttributeProvider.CreateDependencyCollectorContext(node);
                _LayoutContext = new LayoutContext(node, _AttrContext);
            }

            private long _LastInvalidated;
            private IDependencyCollectorAttrContext _AttrContext;
            private LayoutContext _LayoutContext;

            public void SetInvalidated(DateTime t) => _LastInvalidated = t.Ticks;
            public void DoValidation()
            {
                foreach (var child in Node.ChildList)
                    child.RelativeSizeValidator.DoValidation();
                if (_LastInvalidated > Tree.LastValidated)
                {
                    _AttrContext.ReleaseDependencies();
                    Size = Node.Component.Organizer.GetSize(_LayoutContext);
                }
            }
        }
    }
}
