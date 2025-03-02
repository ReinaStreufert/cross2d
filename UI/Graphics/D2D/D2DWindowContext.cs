using SharpDX.Direct2D1;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Graphics.D2D
{
    public class D2DWindowContext : IDisposable
    {
        public static D2DWindowContext CreateFromForm(Form form)
        {
            var dxgiFactory = Direct2D.CreateDXGIFactory();
            var d3dDevice = Direct2D.CreateD3DDevice(dxgiFactory);
            var swapChain = Direct2D.CreateSwapChain(dxgiFactory, d3dDevice, form.Handle, form.Width, form.Height);
            var d2dFactory = Direct2D.CreateD2DFactory();
            var d2dDevice = Direct2D.CreateD2DDevice(d3dDevice, d2dFactory);
            var deviceContext = Direct2D.CreateD2DDeviceContext(d2dDevice);
            var backBufferTarget = Direct2D.CreateD2DTargetFromSwapChain(deviceContext, swapChain);
            var disposables = new IDisposable[] 
            {
                dxgiFactory, d3dDevice, swapChain, d2dFactory, 
                d2dDevice, deviceContext, backBufferTarget
            };
            return new D2DWindowContext(deviceContext, backBufferTarget, swapChain, disposables);
        }

        public DeviceContext DeviceContext { get; }
        public Bitmap1 BackBufferTarget { get; }
        public SwapChain1 SwapChain { get; }

        public D2DWindowContext(DeviceContext deviceContext, Bitmap1 backBufferTarget, SwapChain1 swapChain, IEnumerable<IDisposable> disposables)
        {
            DeviceContext = deviceContext;
            BackBufferTarget = backBufferTarget;
            SwapChain = swapChain;
            _Disposables = disposables;
        }

        private IEnumerable<IDisposable> _Disposables;

        public void Dispose()
        {
            foreach (var disposable in _Disposables)
                disposable.Dispose();
        }
    }
}
