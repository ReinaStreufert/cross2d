using Cross.UI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Events
{
    public class MouseEventArgs : InterruptibleEventArgs
    {
        public Point2DF Location { get; }
        public MouseEventType Type { get; }
        public MouseButtons ButtonsDown { get; }
        public MouseButtons DeltaButtons { get; }
        public int DeltaWheel { get; }

        public MouseEventArgs(IComponent? source, Point2DF location, MouseEventType eventType, MouseButtons buttonsDown, MouseButtons deltaButtons, int deltaWheel) : base(source)
        {
            Location = location;
            Type = eventType;
            ButtonsDown = buttonsDown;
            DeltaButtons = deltaButtons;
            DeltaWheel = deltaWheel;
        }
    }

    public enum MouseEventType
    {
        Down,
        Up,
        Move,
        Enter,
        Leave,
        Wheel,
        Click,
        DoubleClick
    }

    [Flags]
    public enum MouseButtons
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 4
    }
}
