using System;

namespace Encore.Helpers
{
    /// <summary>
    /// A helper class that ensures that a given action is executed only once in a thread-safe manner.
    /// </summary>
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
