using System;
using Newtonsoft.Json.Linq;
using Ripple.Core.Util;

namespace Ripple.Core
{
    public class Uint8 : Uint<byte>
    {
        public Uint8(byte value) : base(value)
        {
        }

        public override byte[] ToBytes()
        {
            return Bits.GetBytes(Value);
        }

        public static Uint8 FromJson(JToken token)
        {
            return (byte) token;
        }
        public static implicit operator Uint8(byte v)
        {
            return new Uint8(v);
        }
    }
}