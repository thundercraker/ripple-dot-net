using System.Threading;

namespace Ripple.Core.ShaMap
{
    internal class Versioner
    {
        private int _value = 0;
        public int IncrementAndGet()
        {
            return ++_value;
        }
    }

    public class ShaMap : ShaMapInner
    {
        private Versioner _copies;

        public ShaMap() : base(0)
        {
            // This way we can copy the first to the second,
            // copy the second, then copy the first again ;)
            _copies = new Versioner();
        }
        public ShaMap(bool isCopy, int depth) : base(isCopy, depth, 0)
        {
        }
        protected internal override ShaMapInner MakeInnerOfSameClass(int depth)
        {
            return new ShaMap(true, depth);
        }
        public virtual ShaMap Copy()
        {
            Version = _copies.IncrementAndGet();
            var copy = (ShaMap)Copy(_copies.IncrementAndGet());
            copy._copies = _copies;
            return copy;
        }
    }
}