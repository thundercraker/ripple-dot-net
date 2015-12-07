using System;
using Ripple.Signing.Utils;
using Sodium;

namespace Ripple.Signing.Ed25519
{
    public class EdKeyPair : IKeyPair
    {
        private readonly KeyPair _pairImpl;
        private byte[] _canonicalisedPubBytes;

        private EdKeyPair(KeyPair pair)
        {
            this._pairImpl = pair;
            ComputeCanonicalPub();
        }

        public byte[] CanonicalPubBytes()
        {
            return _canonicalisedPubBytes;
        }

        private void ComputeCanonicalPub()
        {
            _canonicalisedPubBytes = new byte[33];
            _canonicalisedPubBytes[0] = (byte)0xed;
            Array.Copy(_pairImpl.PublicKey, 0, _canonicalisedPubBytes,
                       1, _pairImpl.PublicKey.Length);
        }

        public byte[] PubKeyHash()
        {
            return HashUtils.PublicKeyHash(CanonicalPubBytes());
        }

        public byte[] Sign(byte[] message)
        {
            return PublicKeyAuth.SignDetached(message, _pairImpl.PrivateKey);
        }

        public bool Verify(byte[] message, byte[] signature)
        {
            return PublicKeyAuth.VerifyDetached(signature, message, _pairImpl.PublicKey);
        }

        internal static IKeyPair From128Seed(byte[] seed)
        {
            var edSecret = new Sha512(seed).Finish256();
            var pair = PublicKeyAuth.GenerateKeyPair(edSecret);
            return new EdKeyPair(pair);
        }

        public string ID()
        {
            return Address.Codec.EncodeAddress(PubKeyHash());
        }
    }
}