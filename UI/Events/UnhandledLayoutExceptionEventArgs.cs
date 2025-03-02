using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.UI.Events
{
    public class UnhandledLayoutExceptionEventArgs : InterruptibleEventArgs
    {
        public Exception Exception { get; }

        public UnhandledLayoutExceptionEventArgs(IComponent? source, Exception exception) : base(source)
        {
            Exception = exception;
        }
    }
}
