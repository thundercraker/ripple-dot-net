using Newtonsoft.Json.Linq;
using Ripple.Core.Util;

namespace Ripple.Core
{
    public class Hash128 : Hash
    {
        public Hash128(byte[] buffer) : base(buffer)
        {
        }

        public static Hash128 FromJson(JToken token)
        {
            return new Hash128(B16.FromHex(token.ToString()));
        }

    }
}