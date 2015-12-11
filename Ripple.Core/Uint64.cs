using System;
using Newtonsoft.Json.Linq;
using Ripple.Core.Util;

namespace Ripple.Core
{
    public class Uint64 : Uint<ulong>
    {
        public Uint64(ulong value) : base(value)
        {
        }
        public override byte[] ToBytes()
        {
            return Bits.GetBytes(Value);
        }

        public override string ToString()
        {
            return B16.ToHex(ToBytes());
        }

        public static Uint64 FromJson(JToken token)
        {
            return Bits.ToUInt64(B16.FromHex(token.ToString()), 0);
        }
        public static implicit operator Uint64(ulong v)
        {
            return new Uint64(v);
        }

        public override JToken ToJson()
        {
            return ToString();
        }
    }
}