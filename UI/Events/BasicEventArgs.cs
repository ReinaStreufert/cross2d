using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Events
{
    public class BasicEventArgs : IEventArgument
    {
        public IComponent? Source { get; }

        public BasicEventArgs(IComponent? source)
        {
            Source = source;
        }
    }
}
