using Cross.UI.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public partial class ComponentTree<TNodeResource, TRenderTarget>
    {
        private class LayoutNode : IComponentTreeNode<TNodeResource>
        {
            public ComponentTree<TNodeResource, TRenderTarget> Tree { get; }
            public IComponent Component { get; }
            public LayoutNode? Parent => _Parent;
            public ComponentChildList ChildList { get; }
            public ChildPlacementValidator PlacementValidator { get; }
            public RelativeSizeValidator SizeValidator { get; }

            public LayoutNode(IComponent component, ComponentTree<TNodeResource, TRenderTarget> tree, LayoutNode? parent)
            {
                Tree = tree;
                Component = component;
                _Parent = parent;
                ChildList = new ComponentChildList(tree, this);
                PlacementValidator = new ChildPlacementValidator(tree, this);
                SizeValidator = new RelativeSizeValidator(tree, this);
            }

            private LayoutNode? _Parent;

            public void SetGraphicsInvalidated(DateTime t)
            {

            }

            IComponentTreeNode<TNodeResource>? IComponentTreeNode<TNodeResource>.Parent => _Parent;

            public IEnumerable<IComponentTreeNode<TNodeResource>> Children => throw new NotImplementedException();

            public TNodeResource Resources => throw new NotImplementedException();

            public Rect2DF OverflowRect => throw new NotImplementedException();

            public Rect2DF ContentRect => throw new NotImplementedException();
            
            IComponentTreeNode? IComponentTreeNode.Parent => throw new NotImplementedException();

            IEnumerable<IComponentTreeNode> IComponentTreeNode.Children => throw new NotImplementedException();

            public bool IsDescendant(IComponent component)
            {
                throw new NotImplementedException();
            }
        }
    }
}
