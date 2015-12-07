namespace Ripple.Crypto
{
    using ECPrivateKeyParameters = Org.BouncyCastle.Crypto.Parameters.ECPrivateKeyParameters;
    using ECPublicKeyParameters = Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters;
    using ECDSASigner = Org.BouncyCastle.Crypto.Signers.ECDsaSigner;
    using ECPoint = Org.BouncyCastle.Math.EC.ECPoint;
    using Org.BouncyCastle.Math;
    using Ripple.Utils;
    using HMacDsaKCalculator = Org.BouncyCastle.Crypto.Signers.HMacDsaKCalculator;
    using Sha256Digest = Org.BouncyCastle.Crypto.Digests.Sha256Digest;

    public class K256KeyGenerator
    {
        // See https://wiki.ripple.com/Account_Family
        public static K256KeyPair From128Seed(byte[] seedBytes, int keyIndex)
        {
            BigInteger secret, privateGen;
            // The private generator (aka root private key, master private key)
            privateGen = ComputePrivateGen(seedBytes);
            if (keyIndex == -1)
            {
                // The root keyPair
                return new K256KeyPair(privateGen);
            }
            else
            {
                secret = ComputeSecretKey(privateGen, (uint) keyIndex);
                return new K256KeyPair(secret);
            }

        }

        public static BigInteger ComputePrivateGen(byte[] seedBytes)
        {
            return ComputeScalar(seedBytes, null);
        }

        public static BigInteger ComputeSecretKey(
            BigInteger privateGen,
            uint accountNumber)
        {
            ECPoint publicGen = ComputePublicGenerator(privateGen);
            return ComputeScalar(publicGen.GetEncoded(true), accountNumber)
                .Add(privateGen)
                .Mod(Secp256k1.Order());
        }

        ///
        /// <param name="privateGen"> secret scalar</param>
        /// <returns> the corresponding public key is the public generator
        ///         (aka public root key, master public key).
        /// </returns>
        public static ECPoint ComputePublicGenerator(BigInteger privateGen)
        {
            return K256KeyGenerator.ComputePublicKey(privateGen);
        }

        public static byte[] ComputePublicKey(byte[] publicGenBytes, uint accountNumber)
        {
            ECPoint rootPubPoint = Secp256k1.Curve().DecodePoint(publicGenBytes);
            BigInteger scalar = ComputeScalar(publicGenBytes, accountNumber);
            ECPoint point = Secp256k1.BasePoint().Multiply(scalar);
            ECPoint offset = rootPubPoint.Add(point);
            return offset.GetEncoded(true);
        }

        /// <param name="seedBytes"> - a bytes sequence of arbitrary length which will be hashed </param>
        /// <param name="discriminator"> - nullable optional uint32 to hash </param>
        /// <returns> a number between [1, order -1] suitable as a private key
        ///  </returns>
        public static BigInteger ComputeScalar(byte[] seedBytes, uint? discriminator)
        {
            BigInteger key = null;
            for (uint i = 0; i <= 0xFFFFFFFFL; i++)
            {
                Sha512 sha512 = new Sha512(seedBytes);
                if (discriminator != null)
                {
                    sha512.AddU32(discriminator.Value);
                }
                sha512.AddU32(i);
                byte[] keyBytes = sha512.Finish256();
                key = Utils.UBigInt(keyBytes);
                if (key.CompareTo(BigInteger.Zero) == 1 &&
                    key.CompareTo(Secp256k1.Order()) == -1)
                {
                    break;
                }
            }
            return key;
        }

        ///
        /// <param name="secretKey"> secret point on the curve as BigInteger </param>
        /// <returns> corresponding public point </returns>
        public static ECPoint ComputePublicKey(BigInteger secretKey)
        {
            return Secp256k1.BasePoint().Multiply(secretKey);
        }
    }

    public class K256VerifyingKey
    {
        protected ECPoint PubKey;
        protected ECDSASigner Verifier;
        protected byte[] PubKeyBytes;

        public K256VerifyingKey(ECPoint pub)
        {
            PubKey = pub;
            PubKeyBytes = pub.GetEncoded(true);
            SetVerifier(pub);
        }

        public byte[] CanonicalPubBytes()
        {
            return PubKeyBytes;
        }

        public bool Verify(byte[] message, byte[] signature)
        {
            byte[] hash = HashUtils.HalfSha512(message);
            return VerifyHash(hash, signature);
        }

        private bool VerifyHash(byte[] data, byte[] signature)
        {
            ECDSASignature sig = ECDSASignature.DecodeFromDER(signature);
            if (sig == null)
            {
                return false;
            }
            return Verifier.VerifySignature(data, sig.r, sig.s);
        }

        protected void SetVerifier(ECPoint pub)
        {
            Verifier = new ECDSASigner();
            ECPublicKeyParameters parameters = new ECPublicKeyParameters(
                pub, Secp256k1.Parameters());
            Verifier.Init(false, parameters);
        }
    }

    public class K256KeyPair : K256VerifyingKey, IKeyPair
    {
        private BigInteger _privKey;
        private ECDSASigner _signer;
        private bool _isNodeKey = false;

        public K256KeyPair(BigInteger priv) : this(priv, K256KeyGenerator.ComputePublicKey(priv))
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
            ECPrivateKeyParameters privKey = new ECPrivateKeyParameters(priv, Secp256k1.Parameters());
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
            ECDSASignature sig = CreateECDSASignature(bytes);
            return sig.EncodeToDER();
        }

        private ECDSASignature CreateECDSASignature(byte[] hash)
        {

            BigInteger[] sigs = _signer.GenerateSignature(hash);
            BigInteger r = sigs[0], s = sigs[1];

            BigInteger otherS = Secp256k1.Order().Subtract(s);
            if (s.CompareTo(otherS) == 1)
            {
                s = otherS;
            }

            return new ECDSASignature(r, s);
        }

        public string ID()
        {
            if (_isNodeKey)
            {
                return Address.EncodeNodePublic(CanonicalPubBytes());
            }
            return Address.EncodeAddress(PubKeyHash());
        }
    }

}