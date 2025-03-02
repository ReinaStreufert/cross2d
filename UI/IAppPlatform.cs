using Cross.Threading;
using Cross.UI.Events;
using Cross.UI.Graphics;
using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI
{
    public interface IAppPlatform
    {
        public bool IsSupported { get; }
        public Task<IAppWindow> NewWindowAsync(IPlatformIndependentInitializer independentInitializer, IComponent rootComponent);
    }

    public interface IAppPlatform<TRenderTarget> : IAppPlatform where TRenderTarget : class, IRenderable
    {
        public Task<IAppWindow<TRenderTarget>> NewWindowAsync(ILayoutEventSink<TRenderTarget> eventSink);
    }

    
}
