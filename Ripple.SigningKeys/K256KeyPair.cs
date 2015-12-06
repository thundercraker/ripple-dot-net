namespace Ripple.Crypto
{
    using ECPrivateKeyParameters = Org.BouncyCastle.Crypto.Parameters.ECPrivateKeyParameters;
    using ECPublicKeyParameters = Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters;
    using ECDSASigner = Org.BouncyCastle.Crypto.Signers.ECDsaSigner;
    using ECPoint = Org.BouncyCastle.Math.EC.ECPoint;
    using Org.BouncyCastle.Math;
    using Ripple.Utils;
    using System;

    public class K256KeyPair : IKeyPair
	{
		internal BigInteger _Priv, _Pub;
		internal byte[] PubBytes;

		// See https://wiki.ripple.com/Account_Family
		/// 
		/// <param name="secretKey"> secret point on the curve as BigInteger </param>
		/// <returns> corresponding public point </returns>
		public static byte[] GetPublic(BigInteger secretKey)
		{
			return SECP256K1.BasePointMultipliedBy(secretKey);
		}

		/// 
		/// <param name="privateGen"> secret point on the curve as BigInteger </param>
		/// <returns> the corresponding public key is the public generator
		///         (aka public root key, master public key).
		///         return as byte[] for convenience. </returns>
		public static byte[] ComputePublicGenerator(BigInteger privateGen)
		{
			return GetPublic(privateGen);
		}

		public static BigInteger ComputePublicKey(BigInteger secret)
		{
			return new BigInteger(1, GetPublic(secret));
		}

		public static BigInteger ComputePrivateGen(byte[] seedBytes)
		{
			return GenerateKey(seedBytes, null);
		}

		public static byte[] ComputePublicKey(byte[] publicGenBytes, int accountNumber)
		{
			ECPoint rootPubPoint = SECP256K1.Curve().DecodePoint(publicGenBytes);
			BigInteger scalar = GenerateKey(publicGenBytes, accountNumber);
			ECPoint point = SECP256K1.BasePoint().Multiply(scalar);
			ECPoint offset = rootPubPoint.Add(point);
			return offset.GetEncoded(true);
		}

		public static BigInteger ComputeSecretKey(
            BigInteger privateGen, 
            byte[] publicGenBytes, 
            int accountNumber)
		{
			return GenerateKey(publicGenBytes, accountNumber).Add(privateGen).Mod(SECP256K1.Order());
		}

		/// <param name="seedBytes"> - a bytes sequence of arbitrary length which will be hashed </param>
		/// <param name="discriminator"> - nullable optional uint32 to hash </param>
		/// <returns> a number between [1, order -1] suitable as a private key
		///  </returns>
		public static BigInteger GenerateKey(byte[] seedBytes, int? discriminator)
		{
			BigInteger key = null;
			for (long i = 0; i <= 0xFFFFFFFFL; i++)
			{
				Sha512 sha512 = (new Sha512()).Add(seedBytes);
				if (discriminator != null)
				{
					sha512.AddU32(discriminator.Value);
				}
				sha512.AddU32((int) i);
				byte[] keyBytes = sha512.Finish256();
				key = Utils.UBigInt(keyBytes);
				if (key.CompareTo(BigInteger.Zero) == 1 && key.CompareTo(SECP256K1.Order()) == -1)
				{
					break;
				}
			}
			return key;
		}

		public BigInteger Pub()
		{
			return _Pub;
		}

		public byte[] CanonicalPubBytes()
		{
			return PubBytes;
		}

		public K256KeyPair(BigInteger priv, BigInteger pub)
		{
			this._Priv = priv;
			this._Pub = pub;
			this.PubBytes = pub.ToByteArrayUnsigned();
		}

		public BigInteger Priv()
		{
			return _Priv;
		}

		public bool VerifyHash(byte[] hash, byte[] sigBytes)
		{
			return Verify(hash, sigBytes, _Pub);
		}

		public byte[] SignHash(byte[] bytes)
		{
			return SignHash(bytes, _Priv);
		}

		public bool VerifySignature(byte[] message, byte[] sigBytes)
		{
			byte[] hash = HashUtils.HalfSha512(message);
			return VerifyHash(hash, sigBytes);
		}

		public byte[] SignMessage(byte[] message)
		{
			byte[] hash = HashUtils.HalfSha512(message);
			return SignHash(hash);
		}

		public byte[] Pub160Hash()
		{
			return HashUtils.PublicKeyHash(PubBytes);
		}

		public string CanonicalPubHex()
		{
			return Utils.BigHex(_Pub);
		}

		public string PrivHex()
		{
			return Utils.BigHex(_Priv);
		}

		public static bool Verify(byte[] data, byte[] sigBytes, BigInteger pub)
		{
			ECDSASignature signature = ECDSASignature.DecodeFromDER(sigBytes);
			if (signature == null)
			{
				return false;
			}
			ECDSASigner signer = new ECDSASigner();
			ECPoint pubPoint = SECP256K1.Curve().DecodePoint(pub.ToByteArrayUnsigned());
			ECPublicKeyParameters @params = new ECPublicKeyParameters(pubPoint, SECP256K1.Parameters());
			signer.Init(false, @params);
			return signer.VerifySignature(data, signature.r, signature.s);
		}

		public static byte[] SignHash(byte[] bytes, BigInteger secret)
		{
			ECDSASignature sig = CreateECDSASignature(bytes, secret);
			byte[] der = sig.EncodeToDER();
			if (!ECDSASignature.IsStrictlyCanonical(der))
			{
				throw new System.InvalidOperationException("Signature is not strictly canonical");
			}
			return der;
		}

		private static ECDSASignature CreateECDSASignature(byte[] hash, BigInteger secret)
		{
			ECDSASigner signer = new ECDSASigner();
			ECPrivateKeyParameters privKey = new ECPrivateKeyParameters(secret, SECP256K1.Parameters());
			signer.Init(true, privKey);
			BigInteger[] sigs = signer.GenerateSignature(hash);
			BigInteger r = sigs[0], s = sigs[1];

			BigInteger otherS = SECP256K1.Order().Subtract(s);
			if (s.CompareTo(otherS) == 1)
			{
				s = otherS;
			}

			return new ECDSASignature(r, s);
		}

        public string ID()
        {
            return Ripple.Address.EncodeAddress(Pub160Hash());
        }
    }

}