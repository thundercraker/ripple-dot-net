using System;
using Newtonsoft.Json.Linq;
using Ripple.Core.Util;

namespace Ripple.Core
{
    public class Blob : ISerializedType
    {
        public readonly byte[] Buffer;
        private Blob(byte[] decode)
        {
            this.Buffer = decode;
        }
        public static Blob FromHex(string value)
        {
            return B16.FromHex(value);
        }
        public static implicit operator Blob(byte[] value)
        {
            return new Blob(value);
        }
        public static implicit operator Blob(JToken token)
        {
            return FromJson(token);
        }
        public static Blob FromJson(JToken token)
        {
            return FromHex(token.ToString());
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