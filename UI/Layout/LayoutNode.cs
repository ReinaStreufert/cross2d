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
            public GraphicValidator[] GraphicValidators { get; }
            public GraphicValidator TopLevelGraphic => GraphicValidators[GraphicValidators.Length - 1];
            public TNodeResource Resources { get; }

            public LayoutNode(IComponent component, ComponentTree<TNodeResource, TRenderTarget> tree, LayoutNode? parent)
            {
                Tree = tree;
                Component = component;
                _Parent = parent;
                ChildList = new ComponentChildList(tree, this);
                PlacementValidator = new ChildPlacementValidator(tree, this);
                SizeValidator = new RelativeSizeValidator(tree, this);
                GraphicValidators = component.Graphics
                    .Select((g, i) => new GraphicValidator(tree, this, g, i))
                    .ToArray();
                Resources = tree.NodeInitializer.InitializeNode(this);
                if (parent != null)
                    _Ancestors = new HashSet<IComponent>(parent._Ancestors.Append(parent.Component));
                else
                    _Ancestors = new HashSet<IComponent>();
            }

            private LayoutNode? _Parent;
            private HashSet<IComponent> _Ancestors;

            public void Validate(DirtyRectList dirtyList, IRenderDevice<TRenderTarget> device)
            {
                ChildList.Validate();
                SizeValidator.Validate();
                PlacementValidator.Validate(dirtyList, device);
            }

            public void SetGraphicsInvalidated(DateTime t) => GraphicValidators[0].SetInvalidated(t);
            IComponentTreeNode<TNodeResource>? IComponentTreeNode<TNodeResource>.Parent => _Parent;
            IEnumerable<IComponentTreeNode<TNodeResource>> IComponentTreeNode<TNodeResource>.Children => ChildList;
            Rect2DF IComponentTreeNode.OverflowRect => TopLevelGraphic.GetOverflowRect(PlacementValidator.TopLeft);
            Rect2DF IComponentTreeNode.ContentRect => PlacementValidator.AbsoluteSize.GetContentRect(PlacementValidator.TopLeft);
            IComponentTreeNode? IComponentTreeNode.Parent => _Parent;
            IEnumerable<IComponentTreeNode> IComponentTreeNode.Children => ChildList;
            public bool IsDescendant(IComponent component) => _Ancestors.Contains(component);
        }
    }
}
