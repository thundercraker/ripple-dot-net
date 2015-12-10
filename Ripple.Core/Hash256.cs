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
            return new Hash256(B16.FromHex(token.ToString()));
        }
    }
}