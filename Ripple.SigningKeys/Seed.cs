using System;

namespace Ripple.Crypto
{

    using Org.BouncyCastle.Math;
    using Ripple.Utils;
    using static Ripple.Address;

	public class Seed
	{
		internal readonly byte[] _SeedBytes;
		internal string _Type;

		public Seed(byte[] seedBytes) : this("secp256k1", seedBytes)
		{
		}
		public Seed(string version, byte[] seedBytes)
		{
			this._SeedBytes = seedBytes;
			this._Type = version;
		}

		public override string ToString()
		{
            return EncodeSeed(_SeedBytes, _Type);
		}

		public byte[] Bytes()
		{
			return _SeedBytes;
		}

		public Seed SetEd25519()
		{
			this._Type = "ed25519";
			return this;
		}

		public IKeyPair KeyPair()
		{
			return KeyPair(0);
		}

		public IKeyPair RootKeyPair()
		{
			return KeyPair(-1);
		}

		public IKeyPair KeyPair(int account)
		{
			if (_Type.Equals("ed25519"))
			{
				if (account != 0)
				{
					throw new ArgumentException("`account` is ignored for ed25519");
				}
				return EDKeyPair.From128Seed(_SeedBytes);
			}
			else
			{
				return CreateKeyPair(_SeedBytes, account);
			}

		}
		public static Seed FromBase58(string b58)
		{
            var seed = DecodeSeed(b58);
			return new Seed(seed.Type, seed.Bytes);
		}

		public static Seed FromPassPhrase(string passPhrase)
		{
			return new Seed(PassPhraseToSeedBytes(passPhrase));
		}

		public static byte[] PassPhraseToSeedBytes(string phrase)
		{
            var phraseBytes = System.Text.Encoding.UTF8.GetBytes(phrase.ToCharArray());
		    return (new Sha512(phraseBytes).Finish128());
		}

		public static IKeyPair CreateKeyPair(byte[] seedBytes)
		{
			return CreateKeyPair(seedBytes, 0);
		}

		public static IKeyPair CreateKeyPair(byte[] seedBytes, int accountNumber)
		{
			BigInteger secret, pub, privateGen;
			// The private generator (aka root private key, master private key)
			privateGen = K256KeyPair.ComputePrivateGen(seedBytes);
			byte[] publicGenBytes = K256KeyPair.ComputePublicGenerator(privateGen);

			if (accountNumber == -1)
			{
				// The root keyPair
				return new K256KeyPair(privateGen, Utils.UBigInt(publicGenBytes));
			}
			else
			{
				secret = K256KeyPair.ComputeSecretKey(privateGen, publicGenBytes, accountNumber);
				pub = K256KeyPair.ComputePublicKey(secret);
				return new K256KeyPair(secret, pub);
			}

		}

		public static IKeyPair GetKeyPair(byte[] seedBytes)
		{
			return CreateKeyPair(seedBytes, 0);
		}

		public static IKeyPair GetKeyPair(string b58)
		{
			return GetKeyPair(DecodeK256Seed(b58));
		}
	}



}