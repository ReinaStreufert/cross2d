using Cross.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Graphics
{
    public interface ICompositeDestination<TRenderable> : IValidated where TRenderable : class, IRenderable
    {
        public void SetCompositionSource(ICompositionSource<TRenderable> compositionSource);
    }
}
