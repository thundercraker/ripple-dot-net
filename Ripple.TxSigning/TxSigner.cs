using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ripple.Core;
using Ripple.Signing;
using static Ripple.Core.Util.B16;

namespace Ripple.TxSigning
{
    public class TxSigner
    {
        private readonly IKeyPair _keyPair;

        private TxSigner(IKeyPair keyPair)
        {
            _keyPair = keyPair;
        }
        private TxSigner(string secret) :
            this(Seed.FromBase58(secret).KeyPair())
        {
        }
        public SignedTx SignPodo(object tx)
        {
            return SignJson(JObject.FromObject(tx));
        }
        public SignedTx SignJson(JObject tx)
        {
            var txJson = tx.DeepClone() as JObject;

            txJson[Field.SigningPubKey] = ToHex(_keyPair.CanonicalPubBytes());
            var so = StObject.FromJson(txJson);
            var sig = _keyPair.Sign(so.SigningData());
            txJson[Field.TxnSignature] = ToHex(sig);
            so[Field.TxnSignature] = (Blob)sig;

            var blob = so.Blob();
            var hash = Utils.TransactionId(blob);
            return new SignedTx(hash, ToHex(blob), txJson);
        }
        public static TxSigner FromKeyPair(IKeyPair keyPair)
        {
            return new TxSigner(keyPair);
        }
        public static TxSigner FromSecret(string secret)
        {
            return new TxSigner(secret);
        }

        public static SignedTx SignPodo(object tx, string secret)
        {
            return FromSecret(secret).SignPodo(tx);
        }
        public static SignedTx SignJson(JObject tx, string secret)
        {
            return FromSecret(secret).SignJson(tx);
        }
    }
}
