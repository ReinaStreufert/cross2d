using Cross.UI.Events;
using Cross.UI.Graphics;
using Cross.UI.Graphics.D2D;
using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.WD2DPlatform
{
    public class WD2DForm : Form, IAppWindow<ID2DRenderable>
    {
        public TaskCompletionSource<WD2DForm> OnLoadTaskSource { get; }

        public WD2DForm(ILayoutEventSink<ID2DRenderable> eventSink, TaskCompletionSource<WD2DForm> onLoadTaskSource)
        {
            _EventSink = eventSink;
            KeyPreview = true;
            OnLoadTaskSource = onLoadTaskSource;
        }

        private D2DWindowContext? _WindowContext;
        private D2DCompositor? _Compositor;
        private ILayoutEventSink<ID2DRenderable> _EventSink;
        private bool _IsProgrammaticallyClosing = false;
        private Events.MouseButtons _ButtonState;
        private System.Windows.Forms.MouseEventArgs _LastMouseEvent = new System.Windows.Forms.MouseEventArgs(MouseButtons, 1, 0, 0, 0);
        private Size2DF _LastSize = new Size2DF(0, 0);
        private WindowState _LastWindowState;

        public float PhysicalPixelRatio
        {
            get
            {
                if (_WindowContext == null)
                    throw new InvalidOperationException("Form is not loaded");
                return _WindowContext.DeviceContext.GetPixelRatio();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _WindowContext = D2DWindowContext.CreateFromForm(this);
            _Compositor = new D2DCompositor(_WindowContext);
            _ButtonState = MouseButtons.ToEventButtons();
            _LastSize = ClientSize.ToSize2DF() / PhysicalPixelRatio;
            _EventSink.OnLoaded(this, _LastSize);
            OnLoadTaskSource.SetResult(this);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _LastWindowState = WindowState.ToWindowState();
            _EventSink.OnShown();
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);
            OnMouseEvent(e, MouseEventType.Down);
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);
            OnMouseEvent(e, MouseEventType.Up);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            OnMouseEvent(_LastMouseEvent, MouseEventType.Click);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
            OnMouseEvent(_LastMouseEvent, MouseEventType.DoubleClick);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            OnMouseEvent(_LastMouseEvent, MouseEventType.Enter);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            OnMouseEvent(_LastMouseEvent, MouseEventType.Leave);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            OnKeyboardEvent(e, KeyboardEventType.Down);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            OnKeyboardEvent(e, KeyboardEventType.Up);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            _EventSink.OnKeyInputEvent(e.ToKeyInputEvent());
        }

        protected void OnMouseEvent(System.Windows.Forms.MouseEventArgs e, MouseEventType type)
        {
            _EventSink.OnMouseEvent(e.ToMouseEvent(type, ref _ButtonState, PhysicalPixelRatio));
            _LastMouseEvent = e;
        }

        protected void OnKeyboardEvent(KeyEventArgs e, KeyboardEventType type)
        {
            _EventSink.OnKeyboardEvent(e.ToKeyboardEvent(type));
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (_WindowContext == null)
                return;
            _EventSink.OnWindowResize(ClientSize.ToSize2DF() / PhysicalPixelRatio);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            var newWindowState = WindowState.ToWindowState();
            if (newWindowState != _LastWindowState)
            {
                _LastWindowState = newWindowState;
                _EventSink.OnWindowStateChanged(newWindowState);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_IsProgrammaticallyClosing)
            {
                var interruptibleEvent = new InterruptibleEventArgs(null);
                _EventSink.OnUserClose(interruptibleEvent);
                e.Cancel = !interruptibleEvent.IsPropogated;
            }
            base.OnFormClosing(e);
        }

        public Task<Size2DF> GetSizeAsync()
        {
            var pxRatio = PhysicalPixelRatio;
            return InvokeAsync(() =>
            {
                if (WindowState != FormWindowState.Minimized)
                    _LastSize = ClientSize.ToSize2DF() / pxRatio;
                return _LastSize;
            });
        }

        public Task SetSizeAsync(Size2DF size)
        {
            var pxRatio = PhysicalPixelRatio;
            return InvokeAsync(() =>
            {
                ClientSize = (size * pxRatio).ToWFSize();
            });
        }

        public Task SetSizeAsync(Func<Size2DF, Size2DF> callback)
        {
            var pxRatio = PhysicalPixelRatio;
            return InvokeAsync(() =>
            {
                ClientSize = (callback(ClientSize.ToSize2DF() / pxRatio) * pxRatio).ToWFSize();
            });
        }

        public Task<WindowState> GetWindowStateAsync()
        {
            return InvokeAsync(() =>
            {
                return WindowState.ToWindowState();
            });
        }

        public Task SetWindowStateAsync(WindowState state)
        {
            return InvokeAsync(() =>
            {
                WindowState = state.ToFormWindowState();
            });
        }

        public Task<bool> IsVisibleAsync()
        {
            return InvokeAsync(() =>
            {
                return Visible;
            });
        }

        public Task SetVisibleAsync(bool visible)
        {
            return InvokeAsync(() =>
            {
                Visible = visible;
            });
        }

        public Task CloseAsync()
        {
            _IsProgrammaticallyClosing = true;
            return InvokeAsync(Close);
        }

        public Task<T> InvokeAsync<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            var asyncResult = BeginInvoke(() =>
            {
                tcs.SetResult(func());
            });
            return tcs.Task.ContinueWith((t) =>
            {
                EndInvoke(asyncResult);
                return t.Result;
            });
        }

        public Task InvokeAsync(Action action)
        {
            var tcs = new TaskCompletionSource();
            var asyncResult = BeginInvoke(() =>
            {
                action();
                tcs.SetResult();
            });
            return tcs.Task.ContinueWith((t) =>
            {
                EndInvoke(asyncResult);
            });
        }

        public void SetCompositionSource(ICompositionSource<ID2DRenderable> compositionSource)
        {
            if (_Compositor == null)
                throw new InvalidOperationException("Form is not loaded");
            _Compositor.SetCompositionSource(compositionSource);
        }

        public void Invalidate(DateTime invalidateTime)
        {
            if (_Compositor == null)
                throw new InvalidOperationException("Form is not loaded");
            _Compositor.Invalidate(invalidateTime);
        }

        public Task<DateTime> InvalidateAndWaitAsync()
        {
            if (_Compositor == null)
                throw new InvalidOperationException("Form is not loaded");
            return _Compositor.InvalidateAndWaitAsync();
        }

        public Task<DateTime> InvalidateAndWaitAsync(DateTime invalidateTime)
        {
            if (_Compositor == null)
                throw new InvalidOperationException("Form is not loaded");
            return _Compositor.InvalidateAndWaitAsync(invalidateTime);
        }

        public Task<DateTime> Sync()
        {
            if (_Compositor == null)
                throw new InvalidOperationException();
            return _Compositor.Sync();
        }

        public Task<DateTime> Sync(DateTime minLastValid)
        {
            if (_Compositor == null)
                throw new InvalidOperationException("Form is not loaded");
            return _Compositor.Sync(minLastValid);
        }
    }
}
