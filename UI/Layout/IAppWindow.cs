using Cross.UI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Layout
{
    public interface IAppWindow : IDisposable
    {
        public float PhysicalPixelRatio { get; }
        public Task<Size2DF> GetSizeAsync();
        public Task SetSizeAsync(Size2DF rect);
        public Task SetSizeAsync(Func<Size2DF, Size2DF> callback);
        public Task<WindowState> GetWindowStateAsync();
        public Task SetWindowStateAsync(WindowState state);
        public Task<bool> IsVisibleAsync();
        public Task SetVisibleAsync(bool visible);
        public Task CloseAsync();
    }

    public interface IAppWindow<TRenderTarget> : IAppWindow, ICompositeDestination<TRenderTarget> where TRenderTarget : class, IRenderable
    {
        
    }

    public enum WindowState
    {
        Normal,
        Minimized,
        Maximized
    }
}
