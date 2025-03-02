using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.Threading
{
    public class TwoPriorityLock
    {
        private const int AcquiredBySoleHolder = -1;
        private int _LockHolders = 0;
        private object _SoleHolderRelease = new object();
        private object _SharedHolderRelease = new object();
        private bool SoleHolderWaiting = false;

        public async Task AcquireSoleAsync()
        {
            await Task.Run(() =>
            {
                if (Interlocked.CompareExchange(ref _LockHolders, AcquiredBySoleHolder, 0) != 0)
                {
                    lock (_SoleHolderRelease)
                    {
                        if (Interlocked.CompareExchange(ref _LockHolders, AcquiredBySoleHolder, 0) != 0)
                        {
                            SoleHolderWaiting = true;
                            Monitor.Wait(_SharedHolderRelease);
                            SoleHolderWaiting = false;
                        }
                    }
                }
            });
        }

        public void ReleaseSole()
        {
            _LockHolders = 0;
            Monitor.PulseAll(_SoleHolderRelease);
        }

        public async Task AcquireSharedAsync()
        {
            await Task.Run(() =>
            {
                int beginVal = _LockHolders;
                for (; ;)
                {
                    if (beginVal == AcquiredBySoleHolder || SoleHolderWaiting)
                    {
                        lock (_SoleHolderRelease)
                            while (_LockHolders == AcquiredBySoleHolder || SoleHolderWaiting)
                                Monitor.Wait(_SoleHolderRelease);
                    }
                    var valOnAttempt = Interlocked.CompareExchange(ref _LockHolders, beginVal + 1, beginVal);
                    if (valOnAttempt == beginVal)
                        break;
                    else
                        beginVal = valOnAttempt;
                }
            });
        }

        public void ReleaseShared()
        {
            int beginVal = _LockHolders;
            for (; ;)
            {
                var valOnAttempt = Interlocked.CompareExchange(ref _LockHolders, beginVal - 1, beginVal);
                if (valOnAttempt == beginVal)
                    break;
                else
                    beginVal = valOnAttempt;
            }
            if (SoleHolderWaiting && beginVal == 1)
            {
                int soleHolderOnAttempt = Interlocked.CompareExchange(ref _LockHolders, AcquiredBySoleHolder, 0);
                if (soleHolderOnAttempt == 0)
                    Monitor.Pulse(_SharedHolderRelease);
            }
        }

        public async Task LockedInvokeAsync(Action callback, bool shared = true)
        {
            if (shared)
                await AcquireSharedAsync();
            else
                await AcquireSoleAsync();
            callback();
            if (shared)
                ReleaseShared();
            else
                ReleaseSole();
        }

        public async Task<T> LockedInvokeAsync<T>(Func<T> callback, bool shared = true)
        {
            if (shared)
                await AcquireSharedAsync();
            else
                await AcquireSoleAsync();
            var result = callback();
            if (shared)
                ReleaseShared();
            else
                ReleaseSole();
            return result;
        }

        public async Task LockedInvokeAsync(Func<Task> asyncCallback, bool shared = true)
        {
            if (shared)
                await AcquireSharedAsync();
            else
                await AcquireSoleAsync();
            await asyncCallback();
            if (shared)
                ReleaseShared();
            else
                ReleaseSole();
        }

        public async Task<T> LockedInvokeAsync<T>(Func<Task<T>> asyncCallback, bool shared = true)
        {
            if (shared)
                await AcquireSharedAsync();
            else
                await AcquireSoleAsync();
            var result = await asyncCallback();
            if (shared)
                ReleaseShared();
            else
                ReleaseSole();
            return result;
        }
    }
}
