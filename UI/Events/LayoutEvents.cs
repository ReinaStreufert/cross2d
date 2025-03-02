using Cross.UI.Graphics;
using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Events
{
    public static class LayoutEvents
    {
        public static Key<KeyboardEvent> KeyDown { get; } = new Key<KeyboardEvent>();
        public static Key<KeyboardEvent> KeyUp { get; } = new Key<KeyboardEvent>();
        public static Key<KeyInputEvent> KeyInput { get; } = new Key<KeyInputEvent>();
        public static Key<MouseEvent> MouseDown { get; } = new Key<MouseEvent>();
        public static Key<MouseEvent> MouseUp { get; } = new Key<MouseEvent>();
        public static Key<MouseEvent> MouseMove { get; } = new Key<MouseEvent>();
        public static Key<MouseEvent> MouseEnter { get; } = new Key<MouseEvent>();
        public static Key<MouseEvent> MouseLeave { get; } = new Key<MouseEvent>();
        public static Key<MouseEvent> MouseWheel { get; } = new Key<MouseEvent>(); 
        public static Key<MouseEvent> MouseClick { get; } = new Key<MouseEvent>();
        public static Key<MouseEvent> MouseDoubleClick { get; } = new Key<MouseEvent>();
        public static Key<FocusedEvent> Focused { get; } = new Key<FocusedEvent>();
        public static Key<LostFocusEvent> LostFocus { get; } = new Key<LostFocusEvent>();
        public static Key<WindowResizedEvent> WindowResized { get; } = new Key<WindowResizedEvent>();
        public static Key<WindowStateChangedEvent> WindowStateChanged { get; } = new Key<WindowStateChangedEvent>();
        public static Key<WindowClosingEvent> WindowClosing { get; } = new Key<WindowClosingEvent>();
        public static Key<WindowShownEvent> WindowShown { get; } = new Key<WindowShownEvent>();
        public static Key<UnhandledExceptionEvent> UnhandledException { get; } = new Key<UnhandledExceptionEvent>();

        public static void RegisterEventTypes(IEventRegistry registry)
        {
            registry.RegisterEventType<KeyboardEvent, KeyboardEventArgs>(new KeyboardEvent());
            registry.RegisterEventType<KeyInputEvent, KeyInputEventArgs>(new KeyInputEvent());
            registry.RegisterEventType<MouseEvent, MouseEventArgs>(new MouseEvent());
            registry.RegisterEventType<WindowResizedEvent, WindowResizedEventArgs>(new WindowResizedEvent());
            registry.RegisterEventType<WindowStateChangedEvent, WindowStateChangedEventArgs>(new WindowStateChangedEvent());
            registry.RegisterEventType<WindowClosingEvent, InterruptibleEventArgs>(new WindowClosingEvent());
            registry.RegisterEventType<WindowShownEvent, BasicEventArgs>(new WindowShownEvent());
            registry.RegisterEventType<UnhandledExceptionEvent, UnhandledLayoutExceptionEventArgs>(new UnhandledExceptionEvent());
            registry.RegisterEventType<FocusedEvent, BasicEventArgs>(new FocusedEvent());
            registry.RegisterEventType<LostFocusEvent, BasicEventArgs>(new LostFocusEvent());
        }
    }

    public class KeyboardEvent : BackpropogatedEvent<KeyboardEventArgs> { }
    public class KeyInputEvent : BackpropogatedEvent<KeyInputEventArgs> { }
    public class WindowResizedEvent : BroadcastEvent<WindowResizedEventArgs> { }
    public class WindowStateChangedEvent : BroadcastEvent<WindowStateChangedEventArgs> { }
    public class WindowShownEvent : BroadcastEvent<BasicEventArgs> { }
    public class WindowClosingEvent : BroadcastEvent<InterruptibleEventArgs> { }
    public class FocusedEvent : UnpropogatedEvent<BasicEventArgs> { }
    public class LostFocusEvent : UnpropogatedEvent<BasicEventArgs> { }
    public class UnhandledExceptionEvent : BackpropogatedEvent<UnhandledLayoutExceptionEventArgs> { }
    public class WindowResizedEventArgs : ValueChangedEventArgs<Size2DF>
    {
        public WindowResizedEventArgs(IComponent? source, Size2DF newValue) : base(source, newValue)
        {
        }
    }
    public class WindowStateChangedEventArgs : ValueChangedEventArgs<WindowState>
    {
        public WindowStateChangedEventArgs(IComponent? source, WindowState newValue) : base(source, newValue)
        {
        }
    }
}
