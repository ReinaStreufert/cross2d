using Cross.UI.Graphics;
using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Components
{
    public static class Attributes
    {
        public static Key<ColorRGBA> BackgroundColor { get; } = new Key<ColorRGBA>();
        public static Key<ColorRGBA> Color { get; } = new Key<ColorRGBA>();

        public static Key<Alignment> InnerAlign { get; } = new Key<Alignment>();
        public static Key<Alignment> CrossAlign { get; } = new Key<Alignment>();
        public static Key<Flow> InnerFlow { get; } = new Key<Flow>();
        public static Key<Wrapping> InnerWrap { get; } = new Key<Wrapping>();
        public static Key<SpatialUnit<Size2DF>> Size { get; } = new Key<SpatialUnit<Size2DF>>();
        public static Key<SpatialUnit<Padding2DF>> Margin { get; } = new Key<SpatialUnit<Padding2DF>>();
        public static Key<SpatialUnit<Padding2DF>> Padding { get; } = new Key<SpatialUnit<Padding2DF>>();
    }

    public enum Alignment
    {
        Begin,
        Center,
        End
    }

    public enum Flow
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }

    public enum Wrapping
    {
        WrapItems,
        NoWrap
    }

    public enum SpaceType
    {
        Flow,
        Cross
    }
}
