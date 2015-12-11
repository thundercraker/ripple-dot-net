using Ripple.Core.Util;
using Ripple.Signing.Utils;

namespace Ripple.TxSigning
{
    static internal class Utils
    {
        internal static string TransactionId(byte[] txBlob)
        {
            return B16.ToHex(new Sha512()
                .AddU32((uint) HashPrefix.TransactionId)
                .Add(txBlob)
                .Finish256());
        }
    }
}