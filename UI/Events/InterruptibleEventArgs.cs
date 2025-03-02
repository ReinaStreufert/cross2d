using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Events
{
    public class InterruptibleEventArgs : BasicEventArgs
    {
        public bool IsPropogated => _IsPropogated;

        public InterruptibleEventArgs(IComponent? source) : base(source)
        {
        }

        private bool _IsPropogated = true;

        public void CancelPropogation()
        {
            _IsPropogated = false;
        }
    }
}
