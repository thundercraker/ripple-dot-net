using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ripple.Core;
using Ripple.Core.Enums;
using Ripple.Core.Types;
using Ripple.Signing;
using static Ripple.Core.Util.B16;

namespace Ripple.TxSigning
{
    public class TxSigner
    {
        public const uint CanonicalSigFlag = 0x80000000u;

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
            var so = StObject.FromJson(tx);
            SetCanonicalSigFlag(so);
            so[Field.SigningPubKey] = (Blob) _keyPair.CanonicalPubBytes();
            so[Field.TxnSignature] = (Blob) _keyPair.Sign(so.SigningData());
            var blob = so.ToBytes();
            var hash = Utils.TransactionId(blob);
            return new SignedTx(hash, ToHex(blob), so.ToJsonObject());
        }

        private static void SetCanonicalSigFlag(StObject so)
        {
            var flags = CanonicalSigFlag;
            if (so.Has(Field.Flags))
            {
                flags |= ((Uint32) so[Field.Flags]).Value;
            }
            so[Field.Flags] = (Uint32) flags;
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
