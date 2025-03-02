using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Events
{
    public class ValueChangedEventArgs<T> : BasicEventArgs
    {
        public T NewValue { get; }

        public ValueChangedEventArgs(IComponent? source, T newValue) : base(source)
        {
            NewValue = newValue;
        }
    }
}
