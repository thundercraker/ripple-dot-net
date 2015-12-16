using Newtonsoft.Json.Linq;
using Ripple.Core.Util;

namespace Ripple.Core
{
    public class Hash256 : Hash
    {
        public Hash256(byte[] buffer) : base(buffer)
        {
        }
        public static Hash256 FromJson(JToken token)
        {
            return new Hash256(B16.Decode(token.ToString()));
        }

        public static Hash256 FromParser(BinaryParser parser, int? hint = null)
        {
            return new Hash256(parser.Read(32));
        }

        public int Nibblet(int depth)
        {
            var byteIx = depth > 0 ? depth / 2 : 0;
            int b = Buffer[byteIx];
            if (depth % 2 == 0)
            {
                b = (b & 0xF0) >> 4;
            }
            else
            {
                b = b & 0x0F;
            }
            return b;
        }
    }
}