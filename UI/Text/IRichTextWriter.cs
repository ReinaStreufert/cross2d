using Cross.UI.Graphics;
using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Text
{
    public interface IRichTextWriter
    {
        public void Write(string text);
        public void SetStyleFlags(TextStyle style);
        public void ClearStyleFlags(TextStyle style);
        public void SetTextColor(ColorRGBA textColor);
        public void SetFontSize(SpatialUnit<Vec1DF> size);
        public void SetFontFamily(string fontFamilyName);
    }

    [Flags]
    public enum TextStyle
    {
        None = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Strikethrough = 8
    }
}
