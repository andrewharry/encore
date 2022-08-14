using System;

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
