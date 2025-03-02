using Cross.UI.Graphics;
using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI
{
    public interface IPlatformIndependentInitializer
    {
        public Task<IAppWindow<TRenderTarget>> CreateAppWindowAsync<TRenderTarget>(IAppPlatform<TRenderTarget> platform, IComponent rootComponent) where TRenderTarget : class, IRenderable;
    }
}
