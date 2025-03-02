using Cross.UI.Graphics;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public interface ILayoutOrganizer
    {
        public LayoutSize GetSize(IImmutableAttributeContext context);
        public void OrganizeComponents(ILayoutOrganizerContext context);
    }

    public interface ILayoutContext : IImmutableAttributeContext
    {
        public IEnumerable<ILayoutElement> Elements { get; }
    }

    public interface ILayoutOrganizerContext : IImmutableAttributeContext
    {
        public Size2DF ClientSize { get; }
        public IEnumerable<ILayoutElementOrganizer> Elements { get; }
    }

    public interface ILayoutElement
    {
        public IComponentTreeNode ComponentNode { get; }
        public SpatialUnit<Size2DF> SpatialSize { get; }
    }

    public interface ILayoutElementOrganizer : ILayoutElement
    {
        public Rect2DF ClientRect { get; }
        public void SetTopLeft(Point2DF topLeft);
    }

    public abstract class LayoutSize<TSizeUnit, TPadUnit>
    {
        public TSizeUnit ContentSize { get; }
        public TPadUnit Padding { get; }
        public TPadUnit Margin { get; }

        public LayoutSize(TSizeUnit contentSize, TPadUnit padding, TPadUnit margin)
        {
            ContentSize = contentSize;
            Margin = margin;
            Padding = padding;
        }
    }

    public class LayoutSize : LayoutSize<SpatialUnit<Size2DF>, SpatialUnit<Padding2DF>>, IEnumerable<SpatialUnit<Size2DF>>
    {
        public LayoutSize(SpatialUnit<Size2DF> contentSize, SpatialUnit<Padding2DF> padding, SpatialUnit<Padding2DF> margin) : base(contentSize, padding, margin)
        {
            if (padding.Relativity.Any(r => r == SpatialRelativity.RelativeRemaining))
                throw new ArgumentException($"Pading may not use relative rema");
        }

        private IEnumerable<SpatialUnit<Size2DF>> EnumerateSpatialSizes()
        {
            yield return ContentSize;
            var paddingVal = Padding.Value;
            var paddingRelativity = Padding.Relativity;
            yield return new SpatialUnit<Size2DF>(new Size2DF(paddingVal.Left, paddingVal.Top), paddingRelativity[0], paddingRelativity[1]);
            yield return new SpatialUnit<Size2DF>(new Size2DF(paddingVal.Right, paddingVal.Bottom), paddingRelativity[2], paddingRelativity[3]);
        }

        public AbsoluteLayoutSize ToAbsolute(Size2DF totalAvailable, Size2DF remainingSpace)
        {
            var contentSize = ContentSize.ToAbsolute(totalAvailable, remainingSpace);
        }

        public IEnumerator<SpatialUnit<Size2DF>> GetEnumerator()
        {
            return EnumerateSpatialSizes().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return EnumerateSpatialSizes().GetEnumerator();
        }
    }

    public class AbsoluteLayoutSize : LayoutSize<Size2DF, Padding2DF>
    {
        public AbsoluteLayoutSize(Size2DF contentSize, Padding2DF padding, Padding2DF margin) : base(contentSize, padding, margin)
        {
        }
    }
}
