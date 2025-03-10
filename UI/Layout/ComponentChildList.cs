using Cross.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public partial class ComponentTree<TNodeResource, TRenderTarget>
    {
        private class ComponentChildList : IEnumerable<LayoutNode>
        {
            public ComponentTree<TNodeResource, TRenderTarget> Tree { get; }
            public LayoutNode Node { get; }

            public ComponentChildList(ComponentTree<TNodeResource, TRenderTarget> tree, LayoutNode node)
            {
                Tree = tree;
                Node = node;
                _ChildDict = ImmutableDictionary<IComponent, LayoutNode>.Empty;
                _AttrContext = tree.AttributeProvider.CreateDependencyCollectorContext(node);
                _AttrContext.OnDependencyMutated += DependencyMutation;
            }

            private ImmutableDictionary<IComponent, LayoutNode> _ChildDict;
            private IDependencyCollectorAttrContext _AttrContext;
            private long _LastInvalidated;

            public void SetInvalidated(DateTime t) => InterlockedMath.Max(ref _LastInvalidated, t.Ticks);
            public LayoutNode? TryFromComponent(IComponent component) => _ChildDict.TryGetValue(component, out var val) ? val : null;

            public void Validate()
            {
                if (_LastInvalidated > Tree._LastValidated)
                {
                    _AttrContext.ReleaseDependencies();
                    var generatedChildren = Node.Component.ChildProvider.GetChildren(_AttrContext);
                    var recycledChildNodes = generatedChildren.Select(c =>
                    {
                        if (_ChildDict.TryGetValue(c, out var recycledChild))
                            return recycledChild;
                        else
                            return new LayoutNode(c, Tree, Node);
                    });
                    _ChildDict = recycledChildNodes
                        .ToImmutableDictionary(n => n.Component);
                }
                foreach (var child in this)
                    child.ChildList.Validate();
            }


            private void DependencyMutation()
            {
                SetInvalidated(DateTime.Now);
                Tree._Destination.Invalidate();
            }

            public IEnumerator<LayoutNode> GetEnumerator()
            {
                return _ChildDict.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _ChildDict.Values.GetEnumerator();
            }
        }
    }
}
