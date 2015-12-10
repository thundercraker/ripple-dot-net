namespace Ripple.Core
{
    public abstract class Hash : ISerializedType
    {
        public readonly byte[] Buffer;
        protected Hash(byte[] buffer)
        {
            Buffer = buffer;
        }
        public void ToBytes(IBytesSink sink)
        {
            sink.Add(Buffer);
        }
    }
}