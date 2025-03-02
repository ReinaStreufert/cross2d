using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.Threading
{
    public abstract class Validated : IValidated
    {
        public long LastValid { get; private set; }

        private long LastInvalidated;
        private int startAttempts = 0;
        private object ValidatedEvent = new object();

        public void Invalidate() => Invalidate(DateTime.Now);
        public void Invalidate(DateTime invalidateTime)
        {
            InterlockedMath.Max(ref LastInvalidated, invalidateTime.Ticks);
            if (Interlocked.Increment(ref startAttempts) == 0)
                _ = CheckedInvalidateAsync();
        }

        public async Task<DateTime> InvalidateAndWaitAsync() => await InvalidateAndWaitAsync(DateTime.Now);
        public async Task<DateTime> InvalidateAndWaitAsync(DateTime invalidateTime)
        {
            Invalidate(invalidateTime);
            return await Sync(invalidateTime);
        }

        public async Task<DateTime> Sync() => await Sync(DateTime.Now);
        public async Task<DateTime> Sync(DateTime minLastValid)
        {
            return await Task.Run(() =>
            {
                var ticks = minLastValid.Ticks;
                if (LastValid < ticks)
                {
                    lock (ValidatedEvent)
                    {
                        if (LastValid < ticks)
                            Monitor.Wait(ValidatedEvent);
                    }
                }
                return new DateTime(LastValid);
            });
        }

        private async Task CheckedInvalidateAsync()
        {
            for (; ; )
            {
                while (LastValid < LastInvalidated)
                {
                    LastValid = (await ValidateAsync()).Ticks;
                    Monitor.PulseAll(ValidatedEvent);
                }
                // reset startAttempts, check for race condition
                int beginValue = startAttempts;
                int beforeAttemptValue = Interlocked.CompareExchange(ref startAttempts, 0, beginValue);
                if (beginValue == beforeAttemptValue)
                    break; // there was no race
            }
        }

        protected abstract Task<DateTime> ValidateAsync();
    }
}
