using Cross.UI.Events;
using Cross.UI.Graphics;
using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.WD2DPlatform
{
    public static class WinFormsInterop
    {
        public static Rect2DF ToRect2DF(this RectangleF rectangle) =>
            new Rect2DF(rectangle.Left, rectangle.Top, rectangle.Bottom, rectangle.Right);
        public static Rect2DF ToRect2DF(this Rectangle rectangle) =>
            new Rect2DF(rectangle.Left, rectangle.Top, rectangle.Bottom, rectangle.Right);
        public static Rectangle ToWFRectangle(this Rect2DF rectangle) =>
            new Rectangle((int)Math.Round(rectangle.Left), (int)Math.Round(rectangle.Top), (int)Math.Round(rectangle.Width), (int)Math.Round(rectangle.Height));
        public static RectangleF ToWFRectangleF(this Rect2DF rectangle) =>
            new RectangleF(rectangle.Left, rectangle.Right, rectangle.Width, rectangle.Height);
        public static Point2DF ToPoint2DF(this Point point) =>
            new Point2DF(point.X, point.Y);
        public static Point2DF ToPoint2DF(this PointF point) =>
            new Point2DF(point.X, point.Y);
        public static Size2DF ToSize2DF(this Size size) =>
            new Size2DF(size.Width, size.Height);
        public static Size2DF ToSize2DF(this SizeF size) =>
            new Size2DF(size.Width, size.Height);
        public static Point ToWFPoint(this Point2DF point) =>
            new Point((int)Math.Round(point.X), (int)Math.Round(point.Y));
        public static PointF ToWFPointF(this Point2DF point) =>
            new PointF(point.X, point.Y);
        public static Size ToWFSize(this Size2DF size) =>
            new Size((int)Math.Round(size.Width), (int)Math.Round(size.Height));
        public static SizeF ToWFSizeF(this Size2DF size) =>
            new SizeF(size.Width, size.Height);

        public static WindowState ToWindowState(this FormWindowState state)
        {
            return state switch
            {
                FormWindowState.Normal => WindowState.Normal,
                FormWindowState.Minimized => WindowState.Minimized,
                FormWindowState.Maximized => WindowState.Maximized,
                _ => throw new NotImplementedException()
            };
        }

        public static FormWindowState ToFormWindowState(this WindowState state)
        {
            return state switch
            {
                WindowState.Normal => FormWindowState.Normal,
                WindowState.Minimized => FormWindowState.Minimized,
                WindowState.Maximized => FormWindowState.Maximized,
                _ => throw new NotImplementedException()
            };
        }

        public static Events.MouseButtons ToEventButtons(this System.Windows.Forms.MouseButtons buttons)
        {
            var result = Events.MouseButtons.None;
            if (buttons.HasFlag(System.Windows.Forms.MouseButtons.Left))
                result |= Events.MouseButtons.Left;
            if (buttons.HasFlag(System.Windows.Forms.MouseButtons.Middle))
                result |= Events.MouseButtons.Middle;
            if (buttons.HasFlag(System.Windows.Forms.MouseButtons.Right))
                result |= Events.MouseButtons.Right;
            return result;
        }

        public static Events.MouseEventArgs ToMouseEvent(this System.Windows.Forms.MouseEventArgs e, MouseEventType type, ref Events.MouseButtons buttonState, float pxRatio)
        {
            var deltaButtons = e.Button.ToEventButtons();
            if (type == MouseEventType.Down)
                buttonState |= deltaButtons;
            else if (type == MouseEventType.Up)
                buttonState ^= deltaButtons;
            var location = e.Location.ToPoint2DF() / pxRatio;
            var deltaWheel = e.Delta / SystemInformation.MouseWheelScrollDelta;
            return new Events.MouseEventArgs(null, location, type, buttonState, deltaButtons, deltaWheel);
        }

        public static KeyboardEventArgs ToKeyboardEvent(this KeyEventArgs e, KeyboardEventType type)
        {
            return new KeyboardEventArgs(null, (Events.Keys)e.KeyData, type);
        }

        public static KeyInputEventArgs ToKeyInputEvent(this KeyPressEventArgs e)
        {
            return new KeyInputEventArgs(null, e.KeyChar);
        }
    }
}
