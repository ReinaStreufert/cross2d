using Cross.UI.Graphics;
using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Components
{
    public class FlowLayout : ILayoutOrganizer
    {
        public LayoutSize GetSize(ILayoutContext context)
        {
            var padding = context.GetAttributeOrDefault(Attributes.Padding, SpatialUnit<Padding2DF>.Absolute(new Padding2DF()));
            var margin = context.GetAttributeOrDefault(Attributes.Margin, SpatialUnit<Padding2DF>.Absolute(new Padding2DF()));
            if (context.TryGetAttribute(Attributes.Size, out var size))
                return new LayoutSize(size!, padding, margin);
            else
                return new LayoutSize(
                    SpatialUnit<Size2DF>.RelativeTotal(new Size2DF(1f, 1f)), 
                    SpatialUnit<Padding2DF>.Absolute(new Padding2DF()),
                    SpatialUnit<Padding2DF>.Absolute(new Padding2DF()));
        }

        public void OrganizeComponents(ILayoutOrganizerContext context)
        {
            var innerFlow = context.GetAttributeOrDefault(Attributes.InnerFlow, Flow.LeftToRight);
            var innerAlign = context.GetAttributeOrDefault(Attributes.InnerAlign, Alignment.Begin);
            var clientSize = context.ClientSize;
            FlowOrganization organization = innerFlow switch
            {
                Flow.LeftToRight => new HorizontalOrganization(clientSize, innerAlign, false),
                Flow.RightToLeft => new HorizontalOrganization(clientSize, innerAlign, true),
                Flow.TopToBottom => new VerticalOrganization(clientSize, innerAlign, false),
                Flow.BottomToTop => new VerticalOrganization(clientSize, innerAlign, true),
                _ => throw new NotImplementedException()
            };
            organization.AddRange(context.Elements);
            organization.Organize(context);
        }

        private class HorizontalOrganization : FlowOrganization
        {
            public HorizontalOrganization(Size2DF clientSize, Alignment innerAlignment, bool reverse) : base(clientSize, reverse ? Flow.RightToLeft : Flow.LeftToRight, innerAlignment)
            {
                _Reverse = reverse;
            }

            private bool _Reverse;

            protected override Size2DF FlowCrossToWH(Vec1DF flowSpace, Vec1DF crossSpace) => new Size2DF(flowSpace.Value, crossSpace.Value);
            protected override Vec1DF GetSpace(IVectorF vec, SpaceType type) => new Vec1DF(type == SpaceType.Flow ? vec[0] : vec[1]);

            protected override SpatialUnit<Vec1DF> GetSpace<TVec, TUnit>(TUnit unit, SpaceType flow)
            {
                if (flow == SpaceType.Flow)
                    return new SpatialUnit<Vec1DF>(new Vec1DF(unit.Value[0]), unit.Relativity[0]);
                else
                    return new SpatialUnit<Vec1DF>(new Vec1DF(unit.Value[1]), unit.Relativity[1]);
            }

            protected override Point2DF Increment(Point2DF point, Vec1DF offset, SpaceType type)
            {
                if (type == SpaceType.Flow)
                    return new Point2DF(point.X + Directional(offset), point.Y);
                else
                    return new Point2DF(point.X, point.Y + Directional(offset));
            }

            private float Directional(Vec1DF offset) => _Reverse ? -offset.Value : offset.Value;

            
        }

        private class VerticalOrganization : FlowOrganization
        {
            public VerticalOrganization(Size2DF clientSize, Alignment innerAlignment, bool reverse) : base(clientSize, reverse ? Flow.BottomToTop : Flow.TopToBottom, innerAlignment)
            {
            }

            private bool _Reverse;

            protected override Size2DF FlowCrossToWH(Vec1DF flowSpace, Vec1DF crossSpace) => new Size2DF(crossSpace.Value, flowSpace.Value);
            protected override Vec1DF GetSpace(IVectorF vec, SpaceType type) => new Vec1DF(type == SpaceType.Flow ? vec[1] : vec[0]);

            protected override SpatialUnit<Vec1DF> GetSpace<TVec, TUnit>(TUnit unit, SpaceType flow)
            {
                if (flow == SpaceType.Flow)
                    return new SpatialUnit<Vec1DF>(new Vec1DF(unit.Value[1]), unit.Relativity[1]);
                else
                    return new SpatialUnit<Vec1DF>(new Vec1DF(unit.Value[0]), unit.Relativity[0]);
            }

            protected override Point2DF Increment(Point2DF point, Vec1DF offset, SpaceType type)
            {
                if (type == SpaceType.Flow)
                    return new Point2DF(point.X, point.Y + Directional(offset));
                else
                    return new Point2DF(point.X + Directional(offset), point.Y);
            }

            private float Directional(Vec1DF offset) => _Reverse ? -offset.Value : offset.Value;
        }

        private abstract class FlowOrganization
        {
            public Flow InnerFlow { get; }
            public Alignment InnerAlignment { get; }
            public Size2DF ClientSize { get; }

            public FlowOrganization(Size2DF clientSize, Flow innerFlow, Alignment innerAlignment)
            {
                ClientSize = clientSize;
                InnerFlow = innerFlow;
                InnerAlignment = innerAlignment;
            }

            private List<FlowRow> _Rows = new List<FlowRow>();

            public void Add(ILayoutComponentOrganizer organizer)
            {
                if (_Rows.Count == 0 || !_Rows[_Rows.Count - 1].TryAdd(organizer))
                {
                    var newRow = new FlowRow(this, _Rows.Count);
                    _Rows.Add(newRow);
                    newRow.ForceAdd(organizer);
                }
            }

            public void AddRange(IEnumerable<ILayoutComponentOrganizer> organizers)
            {
                foreach (var organizer in organizers)
                    Add(organizer);
            }

            public void Organize(ILayoutOrganizerContext ctx)
            {
                foreach (var row in _Rows)
                    row.Organize(ctx);
            }

            protected abstract Vec1DF GetSpace(IVectorF vec, SpaceType type);
            protected abstract SpatialUnit<Vec1DF> GetSpace<TVec, TUnit>(TUnit unit, SpaceType flow) where TVec : IVectorF<TVec> where TUnit : SpatialUnit<TVec>;
            protected abstract Point2DF Increment(Point2DF point, Vec1DF offset, SpaceType type);
            protected abstract Size2DF FlowCrossToWH(Vec1DF flowSpace, Vec1DF crossSpace);

            private class FlowRow
            {
                public FlowOrganization Organization { get; }
                public IEnumerable<ILayoutComponentOrganizer> Elements => _Elements;
                public SpatialContext<Vec1DF> FlowSpatialContext { get; }

                public FlowRow(FlowOrganization organization, int index)
                {
                    Organization = organization;
                    FlowSpatialContext = new SpatialContext<Vec1DF>(GetSpace(organization.ClientSize, SpaceType.Flow));
                    _Index = index;
                }

                private List<ILayoutComponentOrganizer> _Elements = new List<ILayoutComponentOrganizer>();
                private int _Index;

                public bool TryAdd(ILayoutComponentOrganizer organizer)
                {
                    var flowSpace = organizer.SpatialSize
                        .Select(s => GetSpace<Size2DF, SpatialUnit<Size2DF>>(s, SpaceType.Flow));
                    if (!FlowSpatialContext.CanClaim(flowSpace))
                        return false;
                    FlowSpatialContext.Claim(flowSpace);
                    _Elements.Add(organizer);
                    return true;
                }

                public void ForceAdd(ILayoutComponentOrganizer organizer)
                {
                    var flowSpace = organizer.SpatialSize
                        .Select(s => GetSpace<Size2DF, SpatialUnit<Size2DF>>(s, SpaceType.Flow));
                    FlowSpatialContext.Claim(flowSpace);
                    _Elements.Add(organizer);
                }

                public void Organize(ILayoutOrganizerContext ctx)
                {
                    var rowCrossSpace = GetSpace(ctx.ClientSize, SpaceType.Cross) / Organization._Rows.Count;
                    var current = AlignRow(Increment(new Point2DF(), rowCrossSpace * _Index, SpaceType.Cross));
                    foreach (var element in EnumerateDirectional())
                        current = OrganizeElement(ctx, element, current, rowCrossSpace);
                }

                private Point2DF AlignRow(Point2DF rowStart)
                {
                    var innerAlign = Organization.InnerAlignment;
                    var totalFlow = FlowSpatialContext.TotalSpace;
                    var usedFlow = FlowSpatialContext.SpaceUsed;
                    return innerAlign switch
                    {
                        Alignment.Begin => rowStart,
                        Alignment.Center => Increment(rowStart, totalFlow / 2f - usedFlow / 2f, SpaceType.Flow),
                        Alignment.End => Increment(rowStart, totalFlow - usedFlow, SpaceType.Flow),
                        _ => throw new NotImplementedException()
                    };
                }

                private Point2DF OrganizeElement(ILayoutOrganizerContext ctx, ILayoutComponentOrganizer element, Point2DF cellStart, Vec1DF rowCrossSpace)
                {
                    var totalSpace = FlowCrossToWH(FlowSpatialContext.TotalSpace, rowCrossSpace);
                    var remainingSpace = FlowCrossToWH(FlowSpatialContext.RemainingSpace, rowCrossSpace);
                    var innerFlow = Organization.InnerFlow;
                    var absoluteSize = element.SpatialSize.ToAbsolute(totalSpace, remainingSpace);
                    var crossAlign = ctx.GetAttributeOrDefault(Attributes.CrossAlign, Alignment.Begin);

                    var marginedCrossSpace = GetSpace(absoluteSize.MarginedSize, SpaceType.Cross);
                    var alignedMarginedStart = crossAlign switch
                    {
                        Alignment.Begin => cellStart,
                        Alignment.Center => Increment(cellStart, rowCrossSpace / 2f - marginedCrossSpace / 2f, SpaceType.Cross),
                        Alignment.End => Increment(cellStart, rowCrossSpace - marginedCrossSpace, SpaceType.Cross),
                        _ => throw new NotImplementedException()
                    };
                    element.SetPosition(alignedMarginedStart + absoluteSize.Margin.TopLeft, absoluteSize);
                    var marginedFlowSpace = GetSpace(absoluteSize.MarginedSize, SpaceType.Flow);
                    return Increment(cellStart, marginedFlowSpace, SpaceType.Flow);
                }


                private IEnumerable<ILayoutComponentOrganizer> EnumerateDirectional()
                {
                    var innerFlow = Organization.InnerFlow;
                    if (innerFlow == Flow.RightToLeft || innerFlow == Flow.BottomToTop)
                    {
                        for (int i = _Elements.Count - 1; i >= 0; i--)
                            yield return _Elements[i];
                    } else
                    {
                        foreach (var element in _Elements)
                            yield return element;
                    }
                    // incrementing will always occur from left-to-right or top-to-bottom for simplicity of writing the code.
                    // when the direction is right-to-left or bottom-to-top, the elements are enumerated in reverse
                }

                private Vec1DF GetSpace(IVectorF vec, SpaceType flow) => Organization.GetSpace(vec, flow);
                private SpatialUnit<Vec1DF> GetSpace<TVec, TUnit>(TUnit unit, SpaceType flow) where TVec : IVectorF<TVec> where TUnit : SpatialUnit<TVec>
                    => Organization.GetSpace<TVec, TUnit>(unit, flow);
                private Point2DF Increment(Point2DF point, Vec1DF offset, SpaceType type)
                    => Organization.Increment(point, offset, type);
                private Size2DF FlowCrossToWH(Vec1DF flowSpace, Vec1DF crossSpace) 
                    => Organization.FlowCrossToWH(flowSpace, crossSpace);
                    
            }
        }
    }
}
