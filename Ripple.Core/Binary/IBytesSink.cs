namespace Ripple.Core.Binary
{
    public interface IBytesSink
    {
        void Add(byte aByte);
        void Add(byte[] bytes);
    }
}