using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross.Threading
{
    public static class InterlockedMath
    {
        public static void Max(ref long dst, long value)
        {
            var valBeforeAttempt = dst;
            for (; ; )
            {
                if (valBeforeAttempt >= value)
                    break;
                var valOnAttempt = Interlocked.CompareExchange(ref dst, value, valBeforeAttempt);
                if (valOnAttempt == valBeforeAttempt || valOnAttempt >= value)
                    break;
                else valBeforeAttempt = valOnAttempt;
            }
        }
    }
}
