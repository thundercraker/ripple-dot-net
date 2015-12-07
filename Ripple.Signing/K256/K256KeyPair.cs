using Org.BouncyCastle.Math;
using Ripple.Signing.Utils;

namespace Ripple.Signing.K256
{
    using ECPrivateKeyParameters = Org.BouncyCastle.Crypto.Parameters.ECPrivateKeyParameters;
    using ECDSASigner = Org.BouncyCastle.Crypto.Signers.ECDsaSigner;
    using ECPoint = Org.BouncyCastle.Math.EC.ECPoint;
    using HMacDsaKCalculator = Org.BouncyCastle.Crypto.Signers.HMacDsaKCalculator;
    using Sha256Digest = Org.BouncyCastle.Crypto.Digests.Sha256Digest;

    public class K256KeyPair : K256VerifyingKey, IKeyPair
    {
        private readonly BigInteger _privKey;
        private ECDSASigner _signer;
        private bool _isNodeKey = false;

        public K256KeyPair(BigInteger priv) : 
            this(priv, K256KeyGenerator.ComputePublicKey(priv))
        {
        }

        internal K256KeyPair(BigInteger priv, ECPoint pub) : base(pub)
        {
            _privKey = priv;
            InitSigner(priv);
        }

        private void InitSigner(BigInteger priv)
        {
            _signer = new ECDSASigner(new HMacDsaKCalculator(new Sha256Digest()));
            ECPrivateKeyParameters privKey = new ECPrivateKeyParameters(priv, Secp256K1.Parameters());
            _signer.Init(true, privKey);
        }

        internal K256KeyPair SetNodeKey()
        {
            _isNodeKey = true;
            return this;
        }

        public BigInteger Priv()
        {
            return _privKey;
        }

        public byte[] Sign(byte[] message)
        {
            byte[] hash = HashUtils.HalfSha512(message);
            return SignHash(hash);
        }

        public byte[] PubKeyHash()
        {
            return HashUtils.PublicKeyHash(PubKeyBytes);
        }

        private byte[] SignHash(byte[] bytes)
        {
            EcdsaSignature sig = CreateECDSASignature(bytes);
            return sig.EncodeToDER();
        }

        private EcdsaSignature CreateECDSASignature(byte[] hash)
        {

            BigInteger[] sigs = _signer.GenerateSignature(hash);
            BigInteger r = sigs[0], s = sigs[1];

            BigInteger otherS = Secp256K1.Order().Subtract(s);
            if (s.CompareTo(otherS) == 1)
            {
                s = otherS;
            }

            return new EcdsaSignature(r, s);
        }

        public string ID()
        {
            if (_isNodeKey)
            {
                return Address.AddressCodec.EncodeNodePublic(CanonicalPubBytes());
            }
            return Address.AddressCodec.EncodeAddress(PubKeyHash());
        }
    }

}