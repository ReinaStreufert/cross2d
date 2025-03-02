using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cross.UI.Layout;

namespace Cross.UI.Events
{
    public delegate Task ComponentEventAsyncCallback<TArg>(IUIContext context, TArg argument);
}
