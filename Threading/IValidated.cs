using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.Threading
{
    public interface IValidated
    {
        public void Invalidate();
        public void Invalidate(DateTime invalidateTime);
        public Task<DateTime> InvalidateAndWaitAsync();
        public Task<DateTime> InvalidateAndWaitAsync(DateTime invalidateTime);
        public Task<DateTime> Sync();
        public Task<DateTime> Sync(DateTime minLastValid);
    }
}
