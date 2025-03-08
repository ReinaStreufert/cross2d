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
        private class GraphicValidator
        {
            public ComponentTree<TNodeResource, TRenderTarget> Tree { get; }
            public LayoutNode Node { get; }
            public GraphicValidator? Prev => _ValidatorIndex > 0 ? 
                Node.GraphicValidators[_ValidatorIndex - 1] : null;
            public GraphicValidator? Next => _ValidatorIndex < Node.GraphicValidators.Length - 1 ?
                Node.GraphicValidators[_ValidatorIndex + 1] : null;
            public bool IsInvalid => _LastInvalidated > Tree.LastValidated;

            public GraphicValidator(ComponentTree<TNodeResource, TRenderTarget> tree, LayoutNode node, IComponentGraphic graphic, int validatorIndex)
            {
                Tree = tree;
                Node = node;
                _Graphic = graphic;
                _ValidatorIndex = validatorIndex;
            }

            private IComponentGraphic _Graphic;
            private int _ValidatorIndex;
            private Padding2DF _LastOverflow = new Padding2DF();
            private long _LastInvalidated;
            private TRenderTarget? _Buffer;

            public void SetInvalidated(DateTime t)
            {
                _LastInvalidated = t.Ticks;
                if (Next != null)
                    Next.SetInvalidated(t);
            }

            public void Validate()
            {

            }

            public Rect2DF GetOverflowRect(Point2DF topLeft)
            {
                Rect2DF baseRect;
                if (Prev == null)
                    baseRect = new Rect2DF(topLeft, Node.PlacementValidator.AbsoluteSize.ContentSize);
                else
                    baseRect = Prev.GetOverflowRect(topLeft);
                return baseRect.AddMargin(_LastOverflow);
            }
        }
    }
}
