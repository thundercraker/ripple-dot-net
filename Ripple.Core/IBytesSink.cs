namespace Ripple.Core
{
    public interface IBytesSink
    {
        void Add(byte aByte);
        void Add(byte[] bytes);
    }
}