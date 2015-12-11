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
    }
}