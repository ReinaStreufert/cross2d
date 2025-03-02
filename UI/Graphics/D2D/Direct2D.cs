using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Graphics.D2D
{
    public static class Direct2D
    {
        public static SharpDX.DXGI.Factory2 CreateDXGIFactory() => new SharpDX.DXGI.Factory2();
        public static SharpDX.Direct3D11.Device CreateD3DDevice(SharpDX.DXGI.Factory2 dxgiFactory)
        {
            return new SharpDX.Direct3D11.Device(dxgiFactory.GetAdapter(0), SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport | SharpDX.Direct3D11.DeviceCreationFlags.Debug,
                SharpDX.Direct3D.FeatureLevel.Level_11_1,
                SharpDX.Direct3D.FeatureLevel.Level_11_0,
                SharpDX.Direct3D.FeatureLevel.Level_10_1,
                SharpDX.Direct3D.FeatureLevel.Level_10_0,
                SharpDX.Direct3D.FeatureLevel.Level_9_3,
                SharpDX.Direct3D.FeatureLevel.Level_9_1);
        }

        public static SharpDX.DXGI.SwapChain1 CreateSwapChain(SharpDX.DXGI.Factory2 dxgiFactory, SharpDX.Direct3D11.Device d3dDevice, nint windowHandle, int initialWidth, int initialHeight)
        {
            var swapChainDesc = new SharpDX.DXGI.SwapChainDescription1()
            {
                SwapEffect = SharpDX.DXGI.SwapEffect.Sequential,
                Stereo = false,
                BufferCount = 2,
                AlphaMode = SharpDX.DXGI.AlphaMode.Ignore,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Flags = SharpDX.DXGI.SwapChainFlags.None,
                Width = initialWidth,
                Height = initialHeight,
                Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                Scaling = SharpDX.DXGI.Scaling.Stretch,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0)
            };
            return new SharpDX.DXGI.SwapChain1(dxgiFactory, d3dDevice, windowHandle, ref swapChainDesc, null);
        }

        public static SharpDX.Direct2D1.Factory1 CreateD2DFactory() => new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.MultiThreaded, SharpDX.Direct2D1.DebugLevel.Information);

        public static SharpDX.Direct2D1.Device CreateD2DDevice(SharpDX.Direct3D11.Device d3dDevice, SharpDX.Direct2D1.Factory1 d2dFactory)
        {
            var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device>();
            return new SharpDX.Direct2D1.Device(d2dFactory, dxgiDevice);
        }

        public static SharpDX.Direct2D1.DeviceContext CreateD2DDeviceContext(SharpDX.Direct2D1.Device d2dDevice) => new SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);
        
        public static Bitmap1 CreateD2DTargetFromSwapChain(DeviceContext d2dDeviceContext, SharpDX.DXGI.SwapChain1 dxgiSwapChain)
        {
            var backBuffer = dxgiSwapChain!.GetBackBuffer<SharpDX.DXGI.Surface>(0);
            var bitmapProperties = new BitmapProperties1()
            {
                BitmapOptions = BitmapOptions.CannotDraw | BitmapOptions.Target,
                PixelFormat = new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Ignore)
            };
            return new Bitmap1(d2dDeviceContext, backBuffer, bitmapProperties);
        }

        public static Bitmap1 CreateD2DBuffer(DeviceContext dc, SharpDX.Size2 size)
        {
            var bitmap = new Bitmap1(dc, size);
            return bitmap;
        }

        public static RawRectangleF ToD2DRectF(this Rect2DF rect) => new RawRectangleF(rect.Left, rect.Top, rect.Right, rect.Bottom);
        public static RawRectangle ToD2DRectRound(this Rect2DF rect) => new RawRectangle(
            (int)Math.Round(rect.Left), (int)Math.Round(rect.Top), (int)Math.Round(rect.Right), (int)Math.Round(rect.Bottom));
        public static Size2DF ToSize2DF(this Size2F size) => new Size2DF(size.Width, size.Height);
        public static Size2F ToD2DSizeF(this Size2DF size) => new Size2F(size.Width, size.Height);
        public static Size2 ToD2DSizeRound(this Size2DF size) => new Size2((int)Math.Round(size.Width), (int)Math.Round(size.Height));
        public static RawVector2 ToD2DVector2(this Point2DF point) => new RawVector2(point.X, point.Y);
        public static RawColor4 ToD2DColor4(this ColorRGBA color) => new RawColor4(color.R, color.G, color.B, color.A);

        public static float GetPixelRatio(this DeviceContext dc)
        {
            return dc.PixelSize.Width / dc.Size.Width;
        }
    }
}
