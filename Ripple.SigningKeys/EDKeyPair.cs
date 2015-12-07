using System;
using System.Diagnostics;
using Org.BouncyCastle.Math;
using Sodium;

namespace Ripple.Crypto
{
    using Sodium;

    public class EDKeyPair : IKeyPair
    {
        private KeyPair PairImpl;
        private byte[] CanonicalisedPubBytes;

        private EDKeyPair(KeyPair pair)
        {
            this.PairImpl = pair;
            computeCanonicalPub();
        }

        public byte[] CanonicalPubBytes()
        {
            return CanonicalisedPubBytes;
        }

        private void computeCanonicalPub()
        {
            CanonicalisedPubBytes = new byte[33];
            CanonicalisedPubBytes[0] = (byte)0xed;
            Array.Copy(PairImpl.PublicKey, 0, CanonicalisedPubBytes,
                       1, PairImpl.PublicKey.Length);
        }

        public byte[] PubKeyHash()
        {
            return Utils.HashUtils.PublicKeyHash(CanonicalPubBytes());
        }

        public byte[] Sign(byte[] message)
        {
            return PublicKeyAuth.SignDetached(message, PairImpl.PrivateKey);
        }

        public bool Verify(byte[] message, byte[] signature)
        {
            return PublicKeyAuth.VerifyDetached(signature, message, PairImpl.PublicKey);
        }

        internal static IKeyPair From128Seed(byte[] seed)
        {
            var edSecret = new Ripple.Utils.Sha512(seed).Finish256();
            var pair = PublicKeyAuth.GenerateKeyPair(edSecret);
            return new EDKeyPair(pair);
        }

        public string ID()
        {
            return Ripple.Address.EncodeAddress(PubKeyHash());
        }
    }
}