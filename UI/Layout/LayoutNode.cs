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
        public class LayoutNode : IComponentTreeNode<TNodeResource>
        {
            public ComponentTree<TNodeResource, TRenderTarget> Tree { get; }
            public IComponent Component { get; }
            public ComponentChildList ChildList { get; }
            public RelativeSizeValidator RelativeSizeValidator { get; }

            public LayoutNode(IComponent component, ComponentTree<TNodeResource, TRenderTarget> tree, LayoutNode? parent)
            {
                Tree = tree;
                Component = component;
                _Parent = parent;
            }

            private LayoutNode? _Parent;

            public IComponentTreeNode<TNodeResource>? Parent => throw new NotImplementedException();

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
