﻿using Cross.Threading;
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
        private class RelativeSizeValidator
        {
            public ComponentTree<TNodeResource, TRenderTarget> Tree { get; }
            public LayoutNode Node { get; }
            public LayoutSize Size { get; private set; }

            public RelativeSizeValidator(ComponentTree<TNodeResource, TRenderTarget> tree, LayoutNode node)
            {
                Tree = tree;
                Node = node;
                Size = new LayoutSize(
                    SpatialUnit<Size2DF>.Absolute(new Size2DF()),
                    SpatialUnit<Padding2DF>.Absolute(new Padding2DF()),
                    SpatialUnit<Padding2DF>.Absolute(new Padding2DF()));
                _AttrContext = tree.AttributeProvider.CreateDependencyCollectorContext(node);
                _AttrContext.OnDependencyMutated += DependencyMutationCallback;
                _LayoutContext = new LayoutContext(node, _AttrContext);
            }

            private long _LastInvalidated;
            private IDependencyCollectorAttrContext _AttrContext;
            private LayoutContext _LayoutContext;

            public void SetInvalidated(DateTime t) => InterlockedMath.Max(ref _LastInvalidated, t.Ticks);
            public void Validate()
            {
                foreach (var child in Node.ChildList)
                    child.SizeValidator?.Validate();
                if (_LastInvalidated > Tree._LastValidated)
                {
                    _AttrContext.ReleaseDependencies();
                    Size = Node.Component.Organizer.GetSize(_LayoutContext);
                }
            }

            private void DependencyMutationCallback()
            {
                var t = DateTime.Now;
                SetInvalidated(t);
                Node.PlacementValidator.SetInvalidated(t);
                Node.Parent?.PlacementValidator.SetInvalidated(t);
                Tree._Destination.Invalidate(t);
            }
        }
    }
}
