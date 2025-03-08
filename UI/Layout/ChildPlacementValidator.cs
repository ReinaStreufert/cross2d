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
            }

            private long _LastInvalidated;
            private Point2DF _TopLeft;
            private AbsoluteLayoutSize _AbsoluteSize;

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

                }
            }

            private class LayoutOrganizerContext : ILayoutOrganizerContext
            {
                public Size2DF ClientSize => _Node.PlacementValidator.AbsoluteSize.PaddedSize;
                public IEnumerable<ILayoutComponentOrganizer> Elements => _Organizers;

                public LayoutOrganizerContext(LayoutNode node)
                {
                    _Node = node;
                }

                private LayoutNode _Node;
                private LayoutComponentOrganizer[] _Organizers;


                public T GetAttribute<T>(Key<T> key)
                {
                    throw new NotImplementedException();
                }

                public T GetAttribute<T>(IComponentTreeNode descendant, Key<T> key)
                {
                    throw new NotImplementedException();
                }

                public bool HasAttribute<T>(Key<T> key)
                {
                    throw new NotImplementedException();
                }

                public bool HasAttribute<T>(IComponentTreeNode descendant, Key<T> key)
                {
                    throw new NotImplementedException();
                }

                public bool TryGetAttribute<T>(Key<T> key, out T? val)
                {
                    throw new NotImplementedException();
                }

                public bool TryGetAttribute<T>(IComponentTreeNode descendant, Key<T> key, out T? val)
                {
                    throw new NotImplementedException();
                }
            }

            private class LayoutComponentOrganizer : ILayoutComponentOrganizer
            {
                public AbsoluteLayoutSize Size { get; }
                public LayoutSize SpatialSize => _Node.SizeValidator.Size;
                public IComponentTreeNode ComponentNode => _Node;

                public LayoutComponentOrganizer(LayoutNode node, SpatialContext<Size2DF> spatialContext, Rect2DF parentRect)
                {
                    _Node = node;
                    Size = node.SizeValidator.Size.ToAbsolute(spatialContext.TotalSpace, spatialContext.RemainingSpace);
                    _OldTopLeft = _Node.PlacementValidator.TopLeft;
                    _TopLeft = new Point2DF();
                    _ParentRect = parentRect;
                }

                private LayoutNode _Node;
                private Point2DF _OldTopLeft;
                private Point2DF _TopLeft;
                private Rect2DF _ParentRect;

                public void SetTopLeft(Point2DF topLeft)
                {
                    if (topLeft.X > _ParentRect.Width || topLeft.X < 0 || topLeft.Y > _ParentRect.Height || topLeft.Y < 0)
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
