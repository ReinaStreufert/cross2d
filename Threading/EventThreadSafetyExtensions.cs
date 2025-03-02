using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.Threading
{
    public static class EventThreadSafetyExtensions
    {
        public static void SafeInvoke(this Action? handler)
        {
            // the trick is that handler is now a local.
            // using this is equivelent to inlining:
            // var localEvent = EventProperty;
            // localEvent?.Invoke();
            if (handler != null)
                handler.Invoke();
        }
    }
}
