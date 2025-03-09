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
        private class ChildPlacementValidator
        {
            public ComponentTree<TNodeResource, TRenderTarget> Tree { get; }
            public LayoutNode Node { get; }
            public Point2DF TopLeft => _TopLeft;
            public AbsoluteLayoutSize AbsoluteSize => _AbsoluteSize;

            public ChildPlacementValidator(ComponentTree<TNodeResource, TRenderTarget> tree, LayoutNode node)
            {
                Tree = tree;
                Node = node;
                _TopLeft = new Point2DF();
                _AbsoluteSize = new AbsoluteLayoutSize(new Size2DF(), new Padding2DF(), new Padding2DF());
                _AttrContext = tree.AttributeProvider.CreateDependencyCollectorContext(node);
                _AttrContext.OnDependencyMutated += DependencyMutationCallback;
            }

            private long _LastInvalidated;
            private Point2DF _TopLeft;
            private AbsoluteLayoutSize _AbsoluteSize;
            private IDependencyCollectorAttrContext _AttrContext;

            public void SetInvalidated(DateTime t) => _LastInvalidated = t.Ticks;

            public void Validate(DirtyRectList dirtyList)
            {
                if (_LastInvalidated > Tree.LastValidated)
                {
                    if (Node.Parent == null)
                        _AbsoluteSize = new AbsoluteLayoutSize(Tree.RootSize, new Padding2DF(), new Padding2DF());
                    var spatialContext = new SpatialContext<Size2DF>(_AbsoluteSize.PaddedSize);
                    foreach (var size in Node.ChildList.SelectMany(c => c.SizeValidator.Size))
                        spatialContext.Claim(size);
                    _AttrContext.ReleaseDependencies();
                    var organizerContext = new LayoutOrganizerContext(Node, _AttrContext, spatialContext);
                    Node.Component.Organizer.OrganizeComponents(organizerContext);
                    organizerContext.Apply(dirtyList);
                }
                foreach (var child in Node.ChildList)
                    child.PlacementValidator.Validate(dirtyList);
            }

            private void DependencyMutationCallback()
            {
                SetInvalidated(DateTime.Now);
                Tree._Destination.Invalidate();
            }

            private class LayoutOrganizerContext : ILayoutOrganizerContext
            {
                public Size2DF ClientSize => _Node.PlacementValidator.AbsoluteSize.PaddedSize;
                public IEnumerable<ILayoutComponentOrganizer> Elements => _Organizers;

                public LayoutOrganizerContext(LayoutNode node, IImmutableAttributeContext attrContext, SpatialContext<Size2DF> spatialContext)
                {
                    _Node = node;
                    _AttrContext = attrContext;
                    _Organizers = node.ChildList
                        .Select(n => new LayoutComponentOrganizer(n, spatialContext))
                        .ToArray();
                }

                private LayoutNode _Node;
                private LayoutComponentOrganizer[] _Organizers;
                private IImmutableAttributeContext _AttrContext;

                public T GetAttribute<T>(Key<T> key) => _AttrContext.GetAttribute(key);
                public T GetAttribute<T>(IComponentTreeNode descendant, Key<T> key) => _AttrContext.GetAttribute(key);
                public bool HasAttribute<T>(Key<T> key) => HasAttribute(key);
                public bool HasAttribute<T>(IComponentTreeNode descendant, Key<T> key) => HasAttribute(descendant, key);
                public bool TryGetAttribute<T>(Key<T> key, out T? val) => TryGetAttribute(key, out val);
                public bool TryGetAttribute<T>(IComponentTreeNode descendant, Key<T> key, out T? val) => TryGetAttribute(descendant, key, out val);

                public void Apply(DirtyRectList dirtyList)
                {
                    foreach (var organizer in _Organizers)
                        organizer.Apply(dirtyList);
                }
            }

            private class LayoutComponentOrganizer : ILayoutComponentOrganizer
            {
                public AbsoluteLayoutSize Size { get; }
                public LayoutSize SpatialSize => _Node.SizeValidator.Size;
                public IComponentTreeNode ComponentNode => _Node;

                public LayoutComponentOrganizer(LayoutNode node, SpatialContext<Size2DF> spatialContext)
                {
                    _Node = node;
                    Size = node.SizeValidator.Size.ToAbsolute(spatialContext.TotalSpace, spatialContext.RemainingSpace);
                    _OldTopLeft = _Node.PlacementValidator.TopLeft;
                    _TopLeft = new Point2DF();
                    _ParentSize = spatialContext.TotalSpace;
                }

                private LayoutNode _Node;
                private Point2DF _OldTopLeft;
                private Point2DF _TopLeft;
                private Size2DF _ParentSize;

                public void SetTopLeft(Point2DF topLeft)
                {
                    if (topLeft.X > _ParentSize.Width || topLeft.X < 0 || topLeft.Y >= _ParentSize.Height || topLeft.Y < 0)
                        throw new ArgumentOutOfRangeException(nameof(topLeft));
                    _TopLeft = topLeft;
                }

                public void Apply(DirtyRectList dirtyList)
                {
                    var oldContentSize = _Node.PlacementValidator.AbsoluteSize.ContentSize;
                    Rect2DF? oldOverflowRect = null;
                    if (oldContentSize.Width > 0 && oldContentSize.Height > 0)
                        oldOverflowRect = _Node.TopLevelGraphic.GetOverflowRect(_OldTopLeft);
                    _Node.PlacementValidator._AbsoluteSize = Size;
                    _Node.PlacementValidator._TopLeft = _TopLeft;
                    var graphicWasInvalid = _Node.TopLevelGraphic.IsInvalid;
                    _Node.TopLevelGraphic.Validate();
                    var overflowRect = _Node.TopLevelGraphic.GetOverflowRect(_TopLeft);
                    if (!overflowRect.Equals(oldOverflowRect))
                    {
                        if (oldOverflowRect != null)
                            dirtyList.Dirty(oldOverflowRect);
                        dirtyList.Dirty(overflowRect);
                    }
                    else if (graphicWasInvalid)
                        dirtyList.Dirty(overflowRect);
                }
            }
        }
    }
}
