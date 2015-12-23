using Ripple.Core.Binary;
using Ripple.Core.Hashing;

namespace Ripple.Core.ShaMap
{
    public interface IShaMapItem<out T>
    {
        void ToBytesSink(IBytesSink sink);
        IShaMapItem<T> Copy();
        T Value();
        HashPrefix Prefix();
    }
}