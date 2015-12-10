using System;
using Newtonsoft.Json.Linq;
using Ripple.Core.Util;

namespace Ripple.Core
{
    public class Uint32 : Uint<uint>
    {
        public Uint32(uint value) : base(value)
        {
        }
        public static Uint32 FromJson(JToken token)
        {
            return (uint)token;
        }
        public static implicit operator Uint32(uint v)
        {
            return new Uint32(v);
        }

        public override byte[] ToBytes()
        {
            return Bits.GetBytes(Value);
        }
    }
}