using Cross.Threading;
using Cross.UI.Graphics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public partial class ComponentTree<TNodeResource, TRenderTarget> : ICompositionSource<TRenderTarget> where TNodeResource : IComponentResource where TRenderTarget : class, IRenderable
    {
        public IAttributeProvider AttributeProvider { get; }
        public INodeResourceFactory<TNodeResource> NodeInitializer { get; }
        public IComponentTreeNode<TNodeResource> RootNode => Root;

        public ComponentTree(IComponent rootComponent, IAttributeProvider attributeProvider, Size2DF layoutSize, INodeResourceFactory<TNodeResource> nodeInitializer, IValidated compositionDestination)
        {
            AttributeProvider = attributeProvider;
            RootSize = layoutSize;
            Root = new LayoutNode(rootComponent, this, null);
            _Destination = compositionDestination;
            NodeInitializer = nodeInitializer;
        }

        private long _LastValidated;
        private LayoutNode Root;
        private Size2DF RootSize;
        private long SizeLastInvalidated = 0;
        private IValidated _Destination;

        public void Resize(Size2DF layoutSize)
        {
            DateTime invalTime = DateTime.Now;
            RootSize = layoutSize;
            Root.SetGraphicsInvalidated(invalTime);
            Root.PlacementValidator.SetInvalidated(invalTime);
            InterlockedMath.Max(ref SizeLastInvalidated, invalTime.Ticks);
            _Destination.Invalidate();
        }

        public async Task<CompositionFrame<TRenderTarget>> ComposeFrameAsync(IRenderDevice<TRenderTarget> dc)
        {
            var dirtyRects = new DirtyRectList();
            await AttributeProvider.FreezeWhenSafeAsync();
            var validAsOf = DateTime.Now;
            if (SizeLastInvalidated > _LastValidated)
                dirtyRects.Dirty(RootNode.OverflowRect);
            Root.Validate(dirtyRects, dc);
            if (SizeLastInvalidated > _LastValidated)
                dirtyRects.Dirty(RootNode.OverflowRect);
            _LastValidated = validAsOf.Ticks;
            AttributeProvider.Unfreeze();
            var compositeRects = GetCompositeRects(Root)
                .ToArray();
            var windowRect = new Rect2DF(0, 0, RootSize);
            return new CompositionFrame<TRenderTarget>(windowRect, validAsOf, compositeRects, dirtyRects);
        }

        private IEnumerable<CompositeRect<TRenderTarget>> GetCompositeRects(LayoutNode validation)
        {
            var topLeft = validation.PlacementValidator.TopLeft;
            yield return new CompositeRect<TRenderTarget>(validation.TopLevelGraphic.GetOverflowRect(topLeft), validation.TopLevelGraphic.Buffer);
            foreach (var child in validation.ChildList)
            {
                foreach (var result in GetCompositeRects(child))
                    yield return result;
            }
        }

        private class NewChildNode : IComponentTreeNode
        {
            public IComponent Component { get; }
            public IComponentTreeNode Parent { get; }
            public IEnumerable<IComponentTreeNode> Children => Enumerable.Empty<IComponentTreeNode>();
            public Rect2DF OverflowRect => throw new InvalidOperationException("Not available in the current context");
            public Rect2DF ContentRect => throw new NotImplementedException("Not available in the current context");

            public NewChildNode(IComponent component, IComponentTreeNode parent)
            {
                Component = component;
                Parent = parent;
            }

            public bool IsDescendant(IComponent component)
            {
                return component == Component || component == Parent || Parent.IsDescendant(component);
            }
        }
    }
}
