using Cross.UI.Layout;
using Cross.UI.WD2DPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI
{
    public class CrossApp : ICrossApp
    {
        private static readonly IAppPlatform[] _Platforms = new IAppPlatform[] { new WD2DAppPlatform() };
        private static readonly IPlatformIndependentInitializer _IndependentInitializer = new PlatformIndependentInitializer();

        public static CrossApp Create()
        {
            var supportedPlatform = _Platforms
                .Where(p => p.IsSupported)
                .FirstOrDefault();
            if (supportedPlatform == null)
                throw new PlatformNotSupportedException();
            return new CrossApp(supportedPlatform, _IndependentInitializer);
        }

        private CrossApp(IAppPlatform appPlatform, IPlatformIndependentInitializer platformInitializer)
        {
            _AppPlatform = appPlatform;
            _PlatformInitializer = platformInitializer;
        }

        private IAppPlatform _AppPlatform;
        private IPlatformIndependentInitializer _PlatformInitializer;

        public Task<IAppWindow> NewWindowAsync(IComponent rootComponent)
        {
            return _AppPlatform.NewWindowAsync(_PlatformInitializer, rootComponent);
        }
    }
}
