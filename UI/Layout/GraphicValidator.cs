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
        private class GraphicValidator
        {
            public ComponentTree<TNodeResource, TRenderTarget> Tree { get; }
            public LayoutNode Node { get; }
            public GraphicValidator? Prev => _ValidatorIndex > 0 ? 
                Node.GraphicValidators[_ValidatorIndex - 1] : null;
            public GraphicValidator? Next => _ValidatorIndex < Node.GraphicValidators.Length - 1 ?
                Node.GraphicValidators[_ValidatorIndex + 1] : null;
            public bool IsInvalid => _LastInvalidated > Tree._LastValidated;
            public TRenderTarget Buffer => _Buffer ?? throw new InvalidOperationException();

            public GraphicValidator(ComponentTree<TNodeResource, TRenderTarget> tree, LayoutNode node, IComponentGraphic graphic, int validatorIndex)
            {
                Tree = tree;
                Node = node;
                _Graphic = graphic;
                _ValidatorIndex = validatorIndex;
                _AttrContext = tree.AttributeProvider.CreateDependencyCollectorContext(node);
                _AttrContext.OnDependencyMutated += DependencyMutationCallback;
            }

            private IComponentGraphic _Graphic;
            private int _ValidatorIndex;
            private Padding2DF _LastOverflow = new Padding2DF();
            private long _LastInvalidated;
            private TRenderTarget? _Buffer;
            private IDependencyCollectorAttrContext _AttrContext;

            public void SetInvalidated(DateTime t)
            {
                InterlockedMath.Max(ref _LastInvalidated, t.Ticks);
                if (Next != null)
                    Next.SetInvalidated(t);
            }

            public void Validate(IRenderDevice<TRenderTarget> device)
            {
                Prev?.Validate(device);
                _AttrContext.ReleaseDependencies();
                var inputSize = GetInputSize();
                _LastOverflow = _Graphic.GetOverflow(_AttrContext, inputSize);
                var overflowSize = inputSize.AddMargin(_LastOverflow);
                if (_Buffer == null || _Buffer.Size != overflowSize)
                {
                    if (_Buffer != null)
                        _Buffer.Dispose();
                    _Buffer = device.NewRenderBuffer(overflowSize);
                }
                device.PushRenderTarget(_Buffer);
                device.DrawGraphic(_Graphic, _AttrContext, Prev?._Buffer, new Rect2DF(0, 0, overflowSize));
                device.PopRenderTarget();
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

            public Size2DF GetOverflowSize()
            {
                Size2DF baseSize;
                if (Prev == null)
                    baseSize = Node.PlacementValidator.AbsoluteSize.ContentSize;
                else
                    baseSize = Prev.GetOverflowSize();
                return baseSize.AddMargin(_LastOverflow);
            }

            public Size2DF GetInputSize()
            {
                if (Prev == null)
                    return Node.PlacementValidator.AbsoluteSize.ContentSize;
                else
                    return Prev.GetOverflowSize();
            }

            private void DependencyMutationCallback()
            {
                SetInvalidated(DateTime.Now);
                Tree._Destination.Invalidate();
            }
        }
    }
}
