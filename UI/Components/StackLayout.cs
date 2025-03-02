using Cross.UI.Graphics;
using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Components
{
    public class StackLayout : ILayoutOrganizer
    {
        public IEnumerable<KeyValuePair<IComponent, Rect2DF>> OrganizeComponents(IImmutableAttributeContext context, IEnumerable<IComponentTreeNode> components, Size2DF absoluteSize)
        {
            var innerFlow = context.GetAttributeOrDefault(Attributes.InnerFlow, Flow.LeftToRight);
            var innerWrap = context.GetAttributeOrDefault(Attributes.InnerWrap, Wrapping.NoWrap);
            var horizontalAlign = context.GetAttributeOrDefault(Attributes.InnerHorizontalAlign, HorizontalAlignment.Left);
            var verticalAlign = context.GetAttributeOrDefault(Attributes.InnerVerticalAlign, VerticalAlignment.Top);
            var rowAlign = context.GetAttributeOrDefault(Attributes.RowAlign, innerFlow == Flow.LeftToRight || innerFlow == Flow.RightToLeft ? (Alignment)horizontalAlign : (Alignment)verticalAlign);
            var padding = context.GetAttributeOrDefault(Attributes.Padding, new SpatialUnit<Rect2DF>(new Rect2DF(0f, 0f, 0f, 0f), SpatialRelativity.Absolute));
            if (padding.Relativity == SpatialRelativity.RelativeRemaining)
                throw new ArgumentException("Padding must be absolute or relative to total space");
            var absolutePadding = padding.Relativity == SpatialRelativity.Absolute ? padding.Value 
                : padding.Value * new Rect2DF(absoluteSize.Width, absoluteSize.Height, absoluteSize.Width, absoluteSize.Height);
            var state = new SpatialState(absoluteSize, absolutePadding, innerFlow, innerWrap);
            var spatialRequirements = components.Select(n => new SpatialRequirement(context, n));
            var totalFlowSpace = state.TotalFlowSpace;
            var totalCrossSpace = state.TotalCrossSpace;
            int remainSpaceDistrCount = 0;
            foreach (var req in spatialRequirements)
            {
                if (req.ContentSize.Relativity == SpatialRelativity.RelativeRemaining)
                    remainSpaceDistrCount++;
                if (req.Margin.Relativity == SpatialRelativity.RelativeRemaining)
                    remainSpaceDistrCount++;
                var contentAbsolute = req.GetAbsoluteContentSize(state.TotalPaddedSpace);
                var marginAbsolute = req.GetAbsoluteMargin(state.TotalPaddedSpace);
                var flowSpace = GetSpaceOccupied(contentAbsolute, innerFlow, true) +
                    GetSpaceOccupied(marginAbsolute, innerFlow, true);
                var crossSpace = GetSpaceOccupied(contentAbsolute, innerFlow, false) +
                    GetSpaceOccupied(marginAbsolute, innerFlow, false);
                var currentRow = state.CurrentRow;
                if (innerWrap == Wrapping.WrapItems && currentRow.UsedFlowSpace + flowSpace > totalFlowSpace)
                    currentRow = state.NextRow();
                currentRow.UsedFlowSpace += flowSpace;
                currentRow.UsedCrossSpace = Math.Max(currentRow.UsedCrossSpace, crossSpace);
            }
            state.TallyLastRow();
            var distrRemainingCrossSpace = (state.TotalCrossSpace - state.CrossSpaceUsed) / remainSpaceDistrCount;
            var maxRowFlowSpace = 0f;
            var crossContentTotal = 0f;
            foreach (var row in state.Rows)
            {
                var distrRemainingFlowSpace = (state.TotalFlowSpace - row.UsedFlowSpace) / remainSpaceDistrCount;
                var distrRemainingSpace = innerFlow == Flow.LeftToRight || innerFlow == Flow.RightToLeft ?
                    new Size2DF(distrRemainingFlowSpace, distrRemainingCrossSpace) :
                    new Size2DF(distrRemainingCrossSpace, distrRemainingFlowSpace);
                foreach (var req in spatialRequirements.Where(r => r.ContentSize.Relativity == SpatialRelativity.RelativeRemaining || r.Margin.Relativity == SpatialRelativity.RelativeRemaining))
                {
                    var contentAbsolute = req.GetAbsoluteContentSize(state.TotalPaddedSpace, distrRemainingSpace);
                    var marginAbsolute = req.GetAbsoluteMargin(state.TotalPaddedSpace, distrRemainingSpace);
                    req.FinalContentSize = contentAbsolute;
                    req.FinalMargin = marginAbsolute;
                    var flowSpace = GetSpaceOccupied(contentAbsolute, innerFlow, true) +
                        GetSpaceOccupied(marginAbsolute, innerFlow, true);
                    var crossSpace = GetSpaceOccupied(contentAbsolute, innerFlow, false) +
                        GetSpaceOccupied(marginAbsolute, innerFlow, false);
                    req.FinalFlowSpace = flowSpace;
                    req.FinalCrossSpace = crossSpace;
                    row.UsedFlowSpace += flowSpace;
                    row.UsedCrossSpace = Math.Max(row.UsedCrossSpace, crossSpace);
                }
                maxRowFlowSpace = Math.Max(maxRowFlowSpace, row.UsedFlowSpace);
                crossContentTotal += row.UsedCrossSpace;
            }
            var childContentSpace = innerFlow == Flow.LeftToRight || innerFlow == Flow.RightToLeft ?
                new Size2DF(maxRowFlowSpace, crossContentTotal) :
                new Size2DF(crossContentTotal, maxRowFlowSpace);
            var contentLeft = horizontalAlign switch
            {
                HorizontalAlignment.Left => absolutePadding.Left,
                HorizontalAlignment.Center => absolutePadding.Left + (absolutePadding.Width / 2f) - (childContentSpace.Width / 2f),
                HorizontalAlignment.Right => absolutePadding.Right - childContentSpace.Width,
                _ => throw new NotImplementedException()
            };
            var contentTop = verticalAlign switch
            {
                VerticalAlignment.Top => absolutePadding.Top,
                VerticalAlignment.Center => absolutePadding.Top + (absolutePadding.Height / 2f) - (absolutePadding.Height / 2f),
                VerticalAlignment.Bottom => absolutePadding.Bottom - childContentSpace.Height,
                _ => throw new NotImplementedException()
            };
            var contentRect = new Rect2DF(new Point2DF(contentLeft, contentTop), childContentSpace);
            var currentPosition = innerFlow switch
            {
                Flow.LeftToRight => contentRect.TopLeft,
                Flow.RightToLeft => contentRect.TopRight,
                Flow.TopToBottom => contentRect.TopLeft,
                Flow.BottomToTop => contentRect.BottomLeft,
                _ => throw new NotImplementedException()
            };
            var lastRowCrossSpace = 0f;
            foreach (var row in state.Rows)
            {
                currentPosition = WrapIncrement(currentPosition, contentRect, innerFlow, rowAlign, row, lastRowCrossSpace);
                lastRowCrossSpace = row.UsedCrossSpace;
                foreach (var req in row.Elements)
                {
                    Rect2DF itemRect;
                    if (innerFlow == Flow.BottomToTop || innerFlow == Flow.RightToLeft)
                    {
                        currentPosition = FlowIncrement(currentPosition, req.FinalFlowSpace, innerFlow); // increment comes first if reverse flow
                        itemRect = GetItemRect(currentPosition, row, req, rowAlign, innerFlow);
                        
                    } else
                    {
                        itemRect = GetItemRect(currentPosition, row, req, rowAlign, innerFlow);
                        currentPosition = FlowIncrement(currentPosition, req.FinalFlowSpace, innerFlow);
                    }
                    yield return new KeyValuePair<IComponent, Rect2DF>(req.Node.Component, contentRect);
                }
            }
        }

        private Rect2DF GetItemRect(Point2DF currentPosition, WrapRow row, SpatialRequirement req, Alignment crossAlign, Flow innerFlow)
        {
            var margin = req.FinalMargin!;
            var contentSize = req.FinalContentSize!;
            var crossOffset = crossAlign switch
            {
                Alignment.Begin => 0,
                Alignment.Center => row.UsedCrossSpace / 2f - req.FinalCrossSpace / 2f,
                Alignment.End => row.UsedCrossSpace - req.FinalCrossSpace,
                _ => throw new NotImplementedException()
            };
            var left = currentPosition.X + margin.Left;
            var top = currentPosition.Y + margin.Top;

            if (innerFlow == Flow.LeftToRight || innerFlow == Flow.RightToLeft)
                return new Rect2DF(left, top + crossOffset, contentSize);
            else
                return new Rect2DF(left + crossOffset, top, contentSize);
        }

        private float GetSpaceOccupied(Size2DF size, Flow innerFlow, bool flow = true)
        {
            if (innerFlow == Flow.LeftToRight || innerFlow == Flow.RightToLeft)
                return flow ? size.Width : size.Height;
            else
                return flow ? size.Height : size.Width;
        }

        private float GetSpaceOccupied(Rect2DF rect, Flow innerFlow, bool flow = true)
        {
            if (innerFlow == Flow.LeftToRight || innerFlow == Flow.RightToLeft)
                return flow ? rect.Left + rect.Right : rect.Top + rect.Bottom;
            else
                return flow ? rect.Top + rect.Bottom : rect.Left + rect.Right;
        }

        private Point2DF FlowIncrement(Point2DF currentPosition, float absoluteSpace, Flow innerFlow)
        {
            return innerFlow switch
            {
                Flow.LeftToRight => new Point2DF(currentPosition.X + absoluteSpace, currentPosition.Y),
                Flow.RightToLeft => new Point2DF(currentPosition.X - absoluteSpace, currentPosition.Y),
                Flow.TopToBottom => new Point2DF(currentPosition.X, currentPosition.Y + absoluteSpace),
                Flow.BottomToTop => new Point2DF(currentPosition.X, currentPosition.Y - absoluteSpace),
                _ => throw new NotImplementedException()
            };
        }

        private Point2DF WrapIncrement(Point2DF currentPosition, Rect2DF childContentRect, Flow innerFlow, Alignment rowAlign, WrapRow row, float prevRowCrossSpace)
        {
            if (innerFlow == Flow.LeftToRight)
            {
                var beginEdge = rowAlign switch
                {
                    Alignment.Begin => childContentRect.Left,
                    Alignment.Center => childContentRect.Left + (childContentRect.Width / 2) - (row.UsedFlowSpace / 2),
                    Alignment.End => childContentRect.Right - row.UsedFlowSpace,
                    _ => throw new NotImplementedException()
                };
                return new Point2DF(beginEdge, currentPosition.Y + prevRowCrossSpace);
            }
            else if (innerFlow == Flow.RightToLeft)
            {
                var beginEdge = rowAlign switch
                {
                    Alignment.Begin => childContentRect.Right,
                    Alignment.Center => childContentRect.Left + (childContentRect.Width / 2) - (row.UsedFlowSpace / 2),
                    Alignment.End => childContentRect.Left + row.UsedFlowSpace,
                    _ => throw new NotImplementedException()
                };
                return new Point2DF(beginEdge, currentPosition.Y + prevRowCrossSpace);
            }
            else if (innerFlow == Flow.TopToBottom)
            {
                var beginEdge = rowAlign switch
                {
                    Alignment.Begin => childContentRect.Top,
                    Alignment.Center => childContentRect.Top + (childContentRect.Height / 2) - (row.UsedFlowSpace / 2),
                    Alignment.End => childContentRect.Bottom - row.UsedFlowSpace,
                    _ => throw new NotImplementedException()
                };
                return new Point2DF(currentPosition.X + prevRowCrossSpace, beginEdge);
            }
            else if (innerFlow == Flow.BottomToTop)
            {
                var beginEdge = rowAlign switch
                {
                    Alignment.Begin => childContentRect.Bottom,
                    Alignment.Center => childContentRect.Top + (childContentRect.Height / 2) - (row.UsedFlowSpace / 2),
                    Alignment.End => childContentRect.Top + row.UsedFlowSpace,
                    _ => throw new NotImplementedException()
                };
                return new Point2DF(currentPosition.X + prevRowCrossSpace, beginEdge);
            }
            else throw new NotImplementedException();
        }

        private class SpatialRequirement
        {
            public IComponentTreeNode Node { get; }
            public SpatialUnit<Size2DF> ContentSize { get; }
            public SpatialUnit<Rect2DF> Margin { get; }
            public Alignment CrossAlign { get; }
            public Size2DF? FinalContentSize { get; set; }
            public Rect2DF? FinalMargin { get; set; }
            public float FinalFlowSpace { get; set; }
            public float FinalCrossSpace { get; set; }

            public SpatialRequirement(IImmutableAttributeContext context, IComponentTreeNode node)
            {
                Node = node;
                ContentSize = context.GetAttributeOrDefault(
                    node,
                    Attributes.Size,
                    new SpatialUnit<Size2DF>(new Size2DF(1f, 1f), SpatialRelativity.RelativeRemaining));
                Margin = context.GetAttributeOrDefault(node,
                    Attributes.Margin,
                    new SpatialUnit<Rect2DF>(new Rect2DF(0f, 0f, 0f, 0f), SpatialRelativity.Absolute));
            }

            public Size2DF GetAbsoluteContentSize(Size2DF totalSpaceAvailable)
            {
                if (ContentSize.Relativity == SpatialRelativity.Absolute)
                    return ContentSize.Value;
                else if (ContentSize.Relativity == SpatialRelativity.RelativeTotal)
                    return ContentSize.Value * totalSpaceAvailable;
                else
                    return new Size2DF(0f, 0f);
            }

            public Rect2DF GetAbsoluteMargin(Size2DF totalSpaceAvailable)
            {
                if (Margin.Relativity == SpatialRelativity.Absolute)
                    return Margin.Value;
                else if (ContentSize.Relativity == SpatialRelativity.RelativeTotal)
                    return Margin.Value * new Rect2DF(totalSpaceAvailable.Width, totalSpaceAvailable.Height, totalSpaceAvailable.Width, totalSpaceAvailable.Height);
                else
                    return new Rect2DF(0f, 0f, 0f, 0f);
            }

            public Size2DF GetAbsoluteContentSize(Size2DF totalSpaceAvailable, Size2DF remainingSpace)
            {
                if (ContentSize.Relativity == SpatialRelativity.RelativeRemaining)
                    return ContentSize.Value * remainingSpace;
                else
                    return GetAbsoluteContentSize(totalSpaceAvailable);
            }

            public Rect2DF GetAbsoluteMargin(Size2DF totalSpaceAvailable, Size2DF remainingSpace)
            {
                if (Margin.Relativity == SpatialRelativity.RelativeRemaining)
                    return Margin.Value * new Rect2DF(remainingSpace.Width, remainingSpace.Height, remainingSpace.Width, remainingSpace.Height);
                else
                    return GetAbsoluteMargin(totalSpaceAvailable);
            }
        }

        private class SpatialState
        {
            public Size2DF TotalPaddedSpace { get; }
            public float TotalFlowSpace { get; }
            public float TotalCrossSpace { get; }
            public float CrossSpaceUsed { get; set; }
            public List<WrapRow> Rows { get; } = new List<WrapRow>();
            public WrapRow CurrentRow => Rows[CurrentRowIndex];
            public int CurrentRowIndex { get; set; } = 0;
            public Wrapping InnerWrap { get; }
            
            public SpatialState(Size2DF absoluteSpace, Rect2DF padding, Flow innerFlow, Wrapping innerWrap)
            {
                var paddedSpace = new Size2DF(
                    absoluteSpace.Width - padding.Left - padding.Right,
                    absoluteSpace.Height - padding.Top - padding.Bottom);
                TotalPaddedSpace = paddedSpace;
                if (innerFlow == Flow.LeftToRight)
                {
                    TotalFlowSpace = paddedSpace.Width;
                    TotalCrossSpace = paddedSpace.Height;
                } else
                {
                    TotalCrossSpace = paddedSpace.Width;
                    TotalFlowSpace = paddedSpace.Height;
                }
                InnerWrap = innerWrap;
                Rows.Add(new WrapRow());
            }

            public WrapRow NextRow()
            {
                CrossSpaceUsed += CurrentRow.UsedCrossSpace;
                var row = new WrapRow();
                Rows.Add(row);
                CurrentRowIndex++;
                return row;
            }

            public void TallyLastRow()
            {
                CrossSpaceUsed += CurrentRow.UsedCrossSpace;
            }
        }

        private class WrapRow
        {
            public float UsedFlowSpace { get; set; }
            public float UsedCrossSpace { get; set; }
            public List<SpatialRequirement> Elements { get; } = new List<SpatialRequirement>();

            public WrapRow()
            {

            }
        }
    }
}
