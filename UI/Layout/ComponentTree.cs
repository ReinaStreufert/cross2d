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
            Root = new LayoutValidator(this, rootComponent, new Rect2DF(0, 0, layoutSize.Width, layoutSize.Height), null);
            _Destination = compositionDestination;
            NodeInitializer = nodeInitializer;
        }

        private long LastValidated;        private LayoutValidator Root;
        private Size2DF RootSize;
        private long SizeLastInvalidated = 0;
        private IValidated _Destination;

        public void Resize(Size2DF layoutSize)
        {
            DateTime invalTime = DateTime.Now;
            RootSize = layoutSize;
            InterlockedMath.Max(ref SizeLastInvalidated, invalTime.Ticks);
            _Destination.Invalidate();
        }

        public async Task<CompositionFrame<TRenderTarget>> ComposeFrameAsync(IRenderDevice<TRenderTarget> dc)
        {
            var dirtyRects = new DirtyRectList();
            await AttributeProvider.FreezeWhenSafeAsync();
            var validAsOf = DateTime.Now;
            if (SizeLastInvalidated > LastValidated)
            {
                Root.ContentRect = new Rect2DF(0, 0, RootSize);
                Root.SetInvalidated(validAsOf);
            }
            foreach (var layout in Traverse(Root))
            {
                layout.DoValidation(dirtyRects);
                foreach (var graphic in layout.GraphicChain)
                    graphic.DoValidation(dc, dirtyRects);
            }
            LastValidated = validAsOf.Ticks;
            AttributeProvider.Unfreeze();
            var compositeRects = Traverse(Root)
                .Select(l => new CompositeRect<TRenderTarget>(l.OverflowRect, l.OverflowBuffer!))
                .ToArray();
            var windowRect = new Rect2DF(0, 0, RootSize);
            return new CompositionFrame<TRenderTarget>(windowRect, validAsOf, compositeRects, dirtyRects);
        }

        private IEnumerable<LayoutValidator> Traverse(LayoutValidator validation)
        {
            yield return validation;
            if (validation.LastChildren == null)
                yield break;
            foreach (var recursChild in validation.LastChildren.Values.SelectMany(c => Traverse(c)))
                yield return recursChild;
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
