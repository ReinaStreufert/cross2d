using Cross.Threading;
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

            public void SetInvalidated(DateTime t) => InterlockedMath.Max(ref _LastInvalidated, t.Ticks);

            public void Validate(DirtyRectList dirtyList, IRenderDevice<TRenderTarget> device)
            {
                if (_LastInvalidated > Tree._LastValidated)
                {
                    if (Node.Parent == null)
                        _AbsoluteSize = new AbsoluteLayoutSize(Tree.RootSize, new Padding2DF(), new Padding2DF());
                    _AttrContext.ReleaseDependencies();
                    var organizerContext = new LayoutOrganizerContext(Node, _AttrContext);
                    Node.Component.Organizer.OrganizeComponents(organizerContext);
                    organizerContext.Apply(dirtyList, device);
                }
                foreach (var child in Node.ChildList)
                    child.PlacementValidator.Validate(dirtyList, device);
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

                public LayoutOrganizerContext(LayoutNode node, IImmutableAttributeContext attrContext)
                {
                    _Node = node;
                    _AttrContext = attrContext;
                    var paddingRect = _Node.PlacementValidator.AbsoluteSize.GetPaddedRect(_Node.PlacementValidator.TopLeft);
                    _Organizers = node.ChildList
                        .Select(n => new LayoutComponentOrganizer(n, paddingRect))
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

                public void Apply(DirtyRectList dirtyList, IRenderDevice<TRenderTarget> device)
                {
                    foreach (var organizer in _Organizers)
                        organizer.Apply(dirtyList, device);
                }
            }

            private class LayoutComponentOrganizer : ILayoutComponentOrganizer
            {
                public LayoutSize SpatialSize => _Node.SizeValidator.Size;
                public IComponentTreeNode ComponentNode => _Node;

                public LayoutComponentOrganizer(LayoutNode node, Rect2DF parentRect)
                {
                    _Node = node;
                    _OldTopLeft = _Node.PlacementValidator.TopLeft;
                    _TopLeft = new Point2DF();
                    _ParentRect = parentRect;
                }

                private LayoutNode _Node;
                private Point2DF _OldTopLeft;
                private Point2DF _TopLeft;
                private AbsoluteLayoutSize? _LayoutSize;
                private Rect2DF _ParentRect;

                public void SetPosition(Point2DF topLeft, AbsoluteLayoutSize size)
                {
                    if (topLeft.X > _ParentRect.Width || topLeft.X < 0 || topLeft.Y >= _ParentRect.Height || topLeft.Y < 0)
                        throw new ArgumentOutOfRangeException(nameof(topLeft));
                    _TopLeft = topLeft;
                    _LayoutSize = size;
                }

                public void Apply(DirtyRectList dirtyList, IRenderDevice<TRenderTarget> device)
                {
                    if (_LayoutSize == null)
                        throw new InvalidOperationException("Not all components were positioned by the organizer");
                    var oldContentSize = _Node.PlacementValidator.AbsoluteSize.ContentSize;
                    Rect2DF? oldOverflowRect = null;
                    if (oldContentSize.Width > 0 && oldContentSize.Height > 0)
                        oldOverflowRect = _Node.TopLevelGraphic.GetOverflowRect(_OldTopLeft);
                    _Node.PlacementValidator._AbsoluteSize = _LayoutSize;
                    _Node.PlacementValidator._TopLeft = _ParentRect.TopLeft + _TopLeft;
                    var graphicWasInvalid = _Node.TopLevelGraphic.IsInvalid;
                    _Node.TopLevelGraphic.Validate(device);
                    var overflowRect = _Node.TopLevelGraphic.GetOverflowRect(_ParentRect.TopLeft + _TopLeft);
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
