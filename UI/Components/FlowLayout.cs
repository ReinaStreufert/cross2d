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
            var innerFlow = context.GetAttributeOrDefault(Attributes.InnerFlow, Flow.LeftToRight);
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
            //context.
            throw new NotImplementedException();
        }

        private abstract class FlowOrganization
        {
            public Flow InnerFlow { get; }
            public Alignment InnerAlignment { get; }
            public Size2DF ClientSize { get; }
            public IEnumerable<FlowRow> Rows => _Rows;

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

            protected abstract Vec1DF GetSpace(IVectorF vec, SpaceType type);
            protected abstract SpatialUnit<Vec1DF> GetSpace<TVec, TUnit>(TUnit unit, SpaceType type) where TVec : IVectorF<TVec> where TUnit : SpatialUnit<TVec>;
            protected abstract Point2DF Increment(Point2DF point, Vec1DF offset, SpaceType type);

            public class FlowRow
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

                public void Organize(ILayoutOrganizerContext ctx, Alignment innerAlign)
                {
                    
                    var rowCrossSpace = GetSpace(ctx.ClientSize, SpaceType.Cross) / Organization._Rows.Count;
                    var current = Increment(new Point2DF(), rowCrossSpace * _Index, SpaceType.Cross);

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

                private void OrganizeElement(ILayoutComponentOrganizer element, float rowCrossSpace)
                {
                    var innerFlow = Organization.InnerFlow;

                    var absoluteSize = element.SpatialSize.ToAbsolute(new Size2DF())
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
                
            }
        }
    }
}
