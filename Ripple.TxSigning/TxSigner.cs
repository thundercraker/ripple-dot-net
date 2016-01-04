using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ripple.Core;
using Ripple.Core.Enums;
using Ripple.Core.Transactions;
using Ripple.Core.Types;
using Ripple.Signing;
using Ripple.Core.Util;
// ReSharper disable RedundantArgumentNameForLiteralExpression

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
        public SignedTx SignJson(JObject tx)
        {
            var so = StObject.FromJson(tx, strict: true);
            SetCanonicalSigFlag(so);
            so[Field.SigningPubKey] = _keyPair.CanonicalPubBytes();
            so[Field.TxnSignature] = _keyPair.Sign(so.SigningData());
            TxFormat.Validate(so);

            var blob = so.ToBytes();
            var hash = Utils.TransactionId(blob);
            return new SignedTx(hash, B16.Encode(blob), so.ToJsonObject());
        }

        private static void SetCanonicalSigFlag(StObject so)
        {
            var flags = CanonicalSigFlag;
            if (so.Has(Field.Flags))
            {
                flags |= so[Field.Flags];
            }
            so[Field.Flags] = flags;
        }

        public static TxSigner FromKeyPair(IKeyPair keyPair)
        {
            return new TxSigner(keyPair);
        }
        public static TxSigner FromSecret(string secret)
        {
            return new TxSigner(secret);
        }

        public static SignedTx SignJson(JObject tx, string secret)
        {
            return FromSecret(secret).SignJson(tx);
        }
    }
}
