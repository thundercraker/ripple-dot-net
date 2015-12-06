using System;
using System.Diagnostics;
using Org.BouncyCastle.Math;
using Sodium;

namespace Ripple.Crypto
{
    using Sodium;

    public class EDKeyPair : IKeyPair
    {
        private KeyPair pair;
        private byte[] _canonicalPubBytes;

        private EDKeyPair(KeyPair pair)
        {
            this.pair = pair;
            computeCanonicalPub();
        }

        public byte[] CanonicalPubBytes()
        {
            return _canonicalPubBytes;
        }

        private void computeCanonicalPub()
        {
            _canonicalPubBytes = new byte[33];
            _canonicalPubBytes[0] = (byte) 0xed;
            Array.Copy(pair.PublicKey, 0, _canonicalPubBytes, 
                       1, pair.PublicKey.Length);
        }

        public string CanonicalPubHex()
        {
            throw new NotImplementedException();
        }

        public BigInteger Priv()
        {
            throw new NotImplementedException();
        }

        public string PrivHex()
        {
            throw new NotImplementedException();
        }

        public BigInteger Pub()
        {
            return Utils.Utils.UBigInt(pair.PublicKey);
        }

        public byte[] Pub160Hash()
        {
            return Utils.HashUtils.PublicKeyHash(CanonicalPubBytes());
        }

        public byte[] SignMessage(byte[] message)
        {
            return PublicKeyAuth.SignDetached(message, pair.PrivateKey);
        }

        public bool VerifySignature(byte[] message, byte[] sigBytes)
        {
            return PublicKeyAuth.VerifyDetached(sigBytes, message, pair.PublicKey);
        }

        internal static IKeyPair From128Seed(byte[] _SeedBytes)
        {
            var edSecret = new Ripple.Utils.Sha512(_SeedBytes).Finish256();
            var pair = PublicKeyAuth.GenerateKeyPair(edSecret);
            return new EDKeyPair(pair);
        }

        public string ID()
        {
            return Ripple.Address.EncodeAddress(Pub160Hash());
        }
    }

}