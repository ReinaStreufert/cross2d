using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cross.UI.Events;
using Cross.UI.Graphics;
using Cross.UI.Layout;

namespace Cross.UI
{
    public interface IComponent : IDisposable
    {
        public Key<IComponent>? DocumentKey { get; }
        public ILayoutOrganizer Organizer { get; }
        public IChildComponentProvider ChildProvider { get; }
        public IEnumerable<IComponentGraphic> Graphics { get; }
        public void Initialize(IAttributeContext attributes, IEventBindingContext eventBindingContext);
    }
}
