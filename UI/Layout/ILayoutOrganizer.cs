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
        public LayoutSize GetSize(ILayoutContext context);
        public void OrganizeComponents(ILayoutOrganizerContext context);
    }

    public interface ILayoutContext : IImmutableAttributeContext
    {
        public IEnumerable<ILayoutComponent> Elements { get; }
    }

    public interface ILayoutOrganizerContext : IImmutableAttributeContext
    {
        public Size2DF ClientSize { get; }
        public IEnumerable<ILayoutComponentOrganizer> Elements { get; }
    }

    public interface ILayoutComponent
    {
        public IComponentTreeNode ComponentNode { get; }
        public LayoutSize SpatialSize { get; }
    }

    public interface ILayoutComponentOrganizer : ILayoutComponent
    {
        public AbsoluteLayoutSize Size { get; }
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
            var totalAvailableP = new Padding2DF(totalAvailable.Width, totalAvailable.Height, totalAvailable.Width, totalAvailable.Height);
            var remainingSpaceP = new Padding2DF(remainingSpace.Width, remainingSpace.Height, remainingSpace.Width, remainingSpace.Height);
            var contentSize = ContentSize.ToAbsolute(totalAvailable, remainingSpace);
            var padding = Padding.ToAbsolute(totalAvailableP, remainingSpaceP);
            var margin = Margin.ToAbsolute(totalAvailableP, remainingSpaceP);
            return new AbsoluteLayoutSize(contentSize, padding, margin);
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
        public Size2DF PaddedSize => ContentSize.SubtractPadding(Padding);
        public Size2DF MarginedSize => ContentSize.AddMargin(Margin);

        public AbsoluteLayoutSize(Size2DF contentSize, Padding2DF padding, Padding2DF margin) : base(contentSize, padding, margin)
        {
        }

        public Rect2DF GetContentRect(Point2DF topLeft) => 
            new Rect2DF(topLeft, ContentSize);
        public Rect2DF GetPaddedRect(Point2DF topLeft) => 
            GetContentRect(topLeft).SubtractPadding(Padding);
        public Rect2DF GetMarginedRect(Point2DF topLeft) => 
            GetContentRect(topLeft).AddMargin(Margin);
    }
}
