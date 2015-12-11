using Newtonsoft.Json.Linq;
using Ripple.Core.Util;

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

        public JToken ToJson()
        {
            return ToString();
        }

        public override string ToString()
        {
            return B16.ToHex(Buffer);
        }
    }
}