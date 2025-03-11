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
            public Size2DF ClientSize { get; }
            public IEnumerable<FlowRow> Rows { get; }

            public FlowOrganization(Size2DF clientSize)
            {
                ClientSize = clientSize;
            }

            private List<FlowRow> _Rows = new List<FlowRow>();

            public void Add(ILayoutComponentOrganizer organizer)
            {
                if (_Rows.Count == 0 || !_Rows[_Rows.Count - 1].TryAdd(organizer))
                {
                    var newRow = new FlowRow(this);
                    _Rows.Add(newRow);
                    newRow.ForceAdd(organizer);
                }
            }

            protected abstract Vec1DF GetSpace(IVectorF vec, SpaceType type);
            protected abstract SpatialUnit<Vec1DF> GetSpace<TVec, TUnit>(TUnit unit, SpaceType type) where TVec : IVectorF<TVec> where TUnit : SpatialUnit<TVec>;

            public class FlowRow
            {
                public FlowOrganization Organization { get; }
                public IEnumerable<ILayoutComponentOrganizer> Elements => _Elements;
                public SpatialContext<Vec1DF> FlowSpatialContext { get; }

                public FlowRow(FlowOrganization organization)
                {
                    Organization = organization;
                    FlowSpatialContext = new SpatialContext<Vec1DF>(GetSpace(organization.ClientSize, SpaceType.Flow));
                }

                private List<ILayoutComponentOrganizer> _Elements = new List<ILayoutComponentOrganizer>();

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

                public void ForceAdd(ILayoutComponent organizer)
                {
                    var flowSpace = organizer.SpatialSize
                        .Select(s => GetSpace<Size2DF, SpatialUnit<Size2DF>>(s, SpaceType.Flow));
                    FlowSpatialContext.Claim(flowSpace);
                    _Elements.Add(organizer);
                }

                private Vec1DF GetSpace(IVectorF vec, SpaceType flow) => Organization.GetSpace(vec, flow);
                private SpatialUnit<Vec1DF> GetSpace<TVec, TUnit>(TUnit unit, SpaceType flow) where TVec : IVectorF<TVec> where TUnit : SpatialUnit<TVec>
                    => Organization.GetSpace<TVec, TUnit>(unit, flow);
            }
        }

        
    }
}
