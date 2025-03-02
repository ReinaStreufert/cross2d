using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Events
{
    public class KeyInputEventArgs : InterruptibleEventArgs
    {
        public char KeyChar;

        public KeyInputEventArgs(IComponent? source, char keyChar) : base(source)
        {
            KeyChar = keyChar;
        }
    }
}
