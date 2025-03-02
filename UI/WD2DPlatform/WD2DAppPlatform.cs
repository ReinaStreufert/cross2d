using Cross.UI.Events;
using Cross.UI.Graphics.D2D;
using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.WD2DPlatform
{
    public class WD2DAppPlatform : IAppPlatform<ID2DRenderable>
    {
        public bool IsSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        private int _MainFormCreated = 0;
        private TaskCompletionSource<WD2DForm> _MainFormTCS = new TaskCompletionSource<WD2DForm>();

        public async Task<IAppWindow<ID2DRenderable>> NewWindowAsync(ILayoutEventSink<ID2DRenderable> eventSink)
        {
            TaskCompletionSource<WD2DForm> tcs;
            WD2DForm form;
            if (Interlocked.CompareExchange(ref _MainFormCreated, 1, 0) > 0)
            {
                tcs = new TaskCompletionSource<WD2DForm>();
                var mainForm = await _MainFormTCS.Task;
                _ = mainForm.InvokeAsync(() =>
                {
                    form = new WD2DForm(eventSink, tcs);
                    form.Show();
                });
                
            }
            else
            {
                tcs = _MainFormTCS;
                _ = Task.Factory.StartNew(() =>
                {
                    form = new WD2DForm(eventSink, tcs);
                    Application.Run(form);
                }, TaskCreationOptions.LongRunning);
            }
            return await tcs.Task;
        }

        public async Task<IAppWindow> NewWindowAsync(IPlatformIndependentInitializer independentInitializer, IComponent root)
        {
            var appWindow = await independentInitializer.CreateAppWindowAsync(this, root);
            return appWindow;
        }
    }
}
