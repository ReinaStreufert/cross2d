using Cross.UI.Graphics;
using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Events
{
    public interface ILayoutEventSink<TRenderTarget> where TRenderTarget : class, IRenderable
    {
        public void OnKeyboardEvent(KeyboardEventArgs e);
        public void OnKeyInputEvent(KeyInputEventArgs e);
        public void OnMouseEvent(MouseEventArgs e);
        public void OnWindowResize(Size2DF size);
        public void OnWindowStateChanged(WindowState newState);
        public void OnUserClose(InterruptibleEventArgs e);
        public void OnLoaded(IAppWindow<TRenderTarget> window, Size2DF size);
        public void OnShown();
    }
}
