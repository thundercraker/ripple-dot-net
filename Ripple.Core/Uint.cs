namespace Ripple.Core
{
    public abstract class Uint<T> : ISerializedType
    {
        public readonly T Value;

        protected Uint(T value)
        {
            Value = value;
        }

        public void ToBytes(IBytesSink sink)
        {
            sink.Add(ToBytes());
        }
        public override string ToString()
        {
            return Value.ToString();
        }

        public abstract byte[] ToBytes();
    }
}