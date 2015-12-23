using Ripple.Core.Hashing;
using Ripple.Core.Util;

namespace Ripple.TxSigning
{
    internal static class Utils
    {
        internal static string TransactionId(byte[] txBlob)
        {
            var hash = Half(txBlob);
            return B16.ToHex(hash);
        }

        public static byte[] Half(byte[] txBlob)
        {
            return Sha512.Half(input: txBlob,
                               prefix: (uint) HashPrefix.TransactionId);
        }
    }
}