using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Events
{
    public class KeyboardEventArgs : InterruptibleEventArgs
    {
        public KeyboardEventArgs(IComponent? source, Keys keyData, KeyboardEventType type) : base(source)
        {
            KeyData = keyData;
            Type = type;
        }

        public KeyboardEventType Type { get; }
        public Keys KeyData { get; }
        public Keys KeyCode => KeyData & Keys.KeyCode;
        public Keys Modifiers => KeyData & Keys.Modifiers;
        public bool Control => Modifiers.HasFlag(Keys.Control);
        public bool Shift => Modifiers.HasFlag(Keys.Shift);
        public bool Alt => Modifiers.HasFlag(Keys.Alt);
    }

    public enum KeyboardEventType
    {
        Down,
        Up
    }
}
