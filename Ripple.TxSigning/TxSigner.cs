using Newtonsoft.Json.Linq;
using Ripple.Core;
using Ripple.Signing;
using Ripple.Signing.Utils;
using static Ripple.Core.Util.B16;

namespace Ripple.TxSigning
{
    public class TxSigner
    {
        public static string TransactionId(byte[] txBlob)
        {
            return ToHex(new Sha512()
                            .AddU32((uint) HashPrefix.TransactionId)
                            .Add(txBlob)
                            .Finish256());
        }

        public static SignedTx Sign(JObject tx, string secret)
        {
            var txJson = tx.DeepClone() as JObject; 
            var seed = Seed.FromBase58(secret);
            var keyPair = seed.KeyPair();

            txJson[Field.SigningPubKey] = ToHex(keyPair.CanonicalPubBytes());
            var so = StObject.FromJson(txJson);
            var sig = keyPair.Sign(so.SigningData());
            txJson[Field.TxnSignature] = ToHex(sig);
            so[Field.TxnSignature] = (Blob) sig;

            var blob = so.Blob();
            var hash = TransactionId(blob);

            return new SignedTx(hash, ToHex(blob), txJson);
        }
    }
}
