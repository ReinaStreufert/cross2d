using Cross.Threading;
using Cross.UI.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public partial class ComponentTree<TNodeResource, TRenderTarget>
    {
        private class LayoutValidator : IComponentTreeNode<TNodeResource>
        {
            public LayoutValidator? Parent { get; }

            public ComponentTree<TNodeResource, TRenderTarget> Processor { get; }
            public IComponent Component { get; }
            public IDependencyCollectorAttrContext OrganizerContext { get; }
            public IDependencyCollectorAttrContext ChildProviderContext { get; }
            public ILayoutOrganizer Organizer { get; }
            public IChildComponentProvider ChildProvider { get; }
            public Rect2DF ContentRect { get; set; }
            public Rect2DF OverflowRect => GraphicChain[GraphicChain.Length - 1].LastOverflowRect;
            public TRenderTarget? OverflowBuffer => GraphicChain[GraphicChain.Length - 1].Buffer;
            public ImmutableDictionary<IComponent, LayoutValidator>? LastChildren { get; set; }
            public GraphicValidation[] GraphicChain { get; }
            public TNodeResource Resources { get; }
            public long PlacementLastInvalidated => _PlacementLastInvalidated;
            public long ChildListLastInvalidated => _ChildListLastInvalidated;

            public LayoutValidator(ComponentTree<TNodeResource, TRenderTarget> processor, IComponent component, Rect2DF contentRect, LayoutValidator? parent)
            {
                var now = DateTime.Now;
                Processor = processor;
                Component = component;
                Organizer = component.Organizer;
                ChildProvider = component.ChildProvider;
                OrganizerContext = processor.AttributeProvider.CreateDependencyCollectorContext(this);
                OrganizerContext.OnDependencyMutated += () => DependencyCollectorCallback(false);
                ChildProviderContext = processor.AttributeProvider.CreateDependencyCollectorContext(this);
                ChildProviderContext.OnDependencyMutated += () => DependencyCollectorCallback(true);
                ContentRect = contentRect;
                GraphicChain = Component.Graphics
                    .Select((g, i) => new GraphicValidation(this, i, g, now))
                    .ToArray();
                _PlacementLastInvalidated = now.Ticks;
                _ChildListLastInvalidated = now.Ticks;
                Parent = parent;
                Resources = Processor.NodeInitializer.InitializeNode(this);
                Resources.Initialize();
                _AllParents = new HashSet<IComponent> { Component };
                if (Parent != null)
                {
                    lock (Parent._HashSetLock)
                        _AllParents.UnionWith(Parent._AllParents);
                }
            }

            private long _PlacementLastInvalidated;
            private long _ChildListLastInvalidated;
            private HashSet<IComponent> _AllParents;
            private object _HashSetLock = new object();
            IComponentTreeNode<TNodeResource>? IComponentTreeNode<TNodeResource>.Parent => Parent;
            IEnumerable<IComponentTreeNode<TNodeResource>> IComponentTreeNode<TNodeResource>.Children => LastChildren?.Values ?? Enumerable.Empty<IComponentTreeNode<TNodeResource>>();
            IComponentTreeNode? IComponentTreeNode.Parent => Parent;
            IEnumerable<IComponentTreeNode> IComponentTreeNode.Children => LastChildren?.Values ?? Enumerable.Empty<IComponentTreeNode>();

            public void SetInvalidated(DateTime time, bool includeChildList = false)
            {
                InterlockedMath.Max(ref _PlacementLastInvalidated, time.Ticks);
                if (includeChildList)
                    InterlockedMath.Max(ref _ChildListLastInvalidated, time.Ticks);
            }

            public void DoValidation(DirtyRectList dirtyList)
            {
                var lastValidated = Processor.LastValidated;
                if (_PlacementLastInvalidated < lastValidated)
                {
                    IEnumerable<IComponent> childComponents;
                    bool childComponentsUpdated = _ChildListLastInvalidated > lastValidated || LastChildren == null;
                    if (childComponentsUpdated || LastChildren == null)
                    {
                        ChildProviderContext.ReleaseDependencies();
                        childComponents = Component.ChildProvider.GetChildren(ChildProviderContext);
                    }
                    else
                        childComponents = LastChildren.Values.Select(c => c.Component);
                    OrganizerContext.ReleaseDependencies();
                    var newPlacements = Organizer.OrganizeComponents(OrganizerContext, childComponents.Select(c => new NewChildNode(c, this)), ContentRect.Size);
                    var newChildren = ChildrenFromPlacements(newPlacements, dirtyList)
                        .ToImmutableDictionary(c => c.Component);
                    if (childComponentsUpdated && LastChildren != null)
                    {
                        var releasedChildren = LastChildren
                            .Where(c => !newChildren.ContainsKey(c.Key))
                            .Select(c => c.Value);
                        foreach (var child in releasedChildren)
                            child.Release();
                    }
                    LastChildren = newChildren;
                }
            }

            private void Release()
            {
                OrganizerContext.ReleaseDependencies();
                ChildProviderContext.ReleaseDependencies();
                foreach (var graphic in GraphicChain)
                {
                    graphic.GraphicContext.ReleaseDependencies();
                    graphic.Buffer?.Dispose();
                }
                Component.Dispose();
                Resources.Dispose();
            }

            private IEnumerable<LayoutValidator> ChildrenFromPlacements(IEnumerable<KeyValuePair<IComponent, Rect2DF>> newPlacements, DirtyRectList dirtyList)
            {
                foreach (var pair in newPlacements)
                {
                    var component = pair.Key;
                    var childContentRect = ContentRect + pair.Value;
                    LayoutValidator child;
                    bool childMoved = false;
                    if (LastChildren == null || !LastChildren.TryGetValue(component, out var originalChild))
                        child = new LayoutValidator(Processor, component, childContentRect, this);
                    else
                    {
                        var oldChildContentRect = originalChild.ContentRect;
                        var oldChildRect = originalChild.OverflowRect;
                        child = originalChild;
                        originalChild.ContentRect = childContentRect;
                        if (childContentRect != oldChildContentRect)
                        {
                            dirtyList.Dirty(oldChildRect);
                            childMoved = true;
                        }
                        if (childContentRect.Size != oldChildContentRect.Size)
                        {
                            var childInvalTime = new DateTime(_PlacementLastInvalidated);
                            originalChild.SetInvalidated(childInvalTime);
                            originalChild.GraphicChain[0].SetInvalidated(childInvalTime);
                        }
                    }
                    if (childMoved)
                    {
                        var now = DateTime.Now;
                        foreach (var graphic in child.GraphicChain)
                            graphic.DoOverflowValidation(now);
                        dirtyList.Dirty(child.OverflowRect);
                    }
                    yield return child;
                }
            }

            private void DependencyCollectorCallback(bool includeChildList)
            {
                var t = DateTime.Now;
                SetInvalidated(t, includeChildList);
                Processor._Destination.Invalidate(t);
            }

            public bool IsDescendant(IComponent component)
            {
                lock (_HashSetLock)
                    return _AllParents.Contains(component);
            }
        }
    }
}
