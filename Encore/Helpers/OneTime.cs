using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Encore.Helpers
{
    internal class OneTime
    {
        private readonly object locker = new object();
        public bool First = true;

        public void Guard(Action methodToRun)
        {
            if (!First) return;
            lock (locker) if (First)
                {
                    methodToRun.Invoke();
                    First = false;
                }
        }
    }
}
