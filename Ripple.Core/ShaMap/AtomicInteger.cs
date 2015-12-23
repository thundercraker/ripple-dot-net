using System.Threading;

namespace Ripple.Core.ShaMap
{
    internal class AtomicInteger
    {
        private int _value = 0;
        public int IncrementAndGet()
        {
            Interlocked.Increment(ref _value);
            return _value;
        }
    }
}