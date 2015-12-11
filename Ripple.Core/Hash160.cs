using Newtonsoft.Json.Linq;
using Ripple.Core.Util;

namespace Ripple.Core
{
    public class Hash160: Hash
    {
        public Hash160(byte[] buffer) : base(buffer)
        {
        }
        public static Hash160 FromJson(JToken token)
        {
            return new Hash160(B16.FromHex(token.ToString()));
        }
    }
}