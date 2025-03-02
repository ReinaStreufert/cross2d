using Cross.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI
{
    public interface ICrossApp
    {
        public Task<IAppWindow> NewWindowAsync(IComponent rootComponent);
    }
}
