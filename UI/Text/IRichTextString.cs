using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Text
{
    public interface IRichTextString
    {
        public string? PlainText { get; }
        public void WriteTo(IRichTextWriter textWriter);
    }
}
