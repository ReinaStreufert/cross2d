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
        }
    }
}
