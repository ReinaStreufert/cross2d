using Cross.UI.Graphics;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public interface IComponentTreeNode
    {
        public IComponent Component { get; }
        public IComponentTreeNode? Parent { get; }
        public IEnumerable<IComponentTreeNode> Children { get; }
        public Rect2DF OverflowRect { get; }
        public Rect2DF ContentRect { get; }
        public bool IsDescendant(IComponent component);
    }

    public interface IComponentTreeNode<TResource> : IComponentTreeNode
    {
        public new IComponentTreeNode<TResource>? Parent { get; }
        public new IEnumerable<IComponentTreeNode<TResource>> Children { get; }
        public TResource Resources { get; }
    }

    public interface INodeResourceFactory<TResource> where TResource : IComponentResource
    {
        public TResource InitializeNode(IComponentTreeNode<TResource> node);
    }

    public interface IComponentResource : IDisposable
    {
        public void Initialize();
    }
}
