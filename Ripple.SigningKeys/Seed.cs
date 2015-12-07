using System;

namespace Ripple.Crypto
{

    using Org.BouncyCastle.Math;
    using Ripple.Utils;
    using static Ripple.Address;
    using System.Security.Cryptography;

    public class Seed
    {
        private readonly byte[] SeedBytes;
        private string KeyType = "secp256k1";
        private bool IsNodeKey = false;

        public Seed(byte[] seedBytes) : this("secp256k1", seedBytes)
        {
        }
        public Seed(string version, byte[] seedBytes)
        {
            this.SeedBytes = seedBytes;
            this.KeyType = version;
        }

        public override string ToString()
        {
            return EncodeSeed(SeedBytes, KeyType);
        }

        public byte[] Bytes()
        {
            return SeedBytes;
        }

        public Seed SetEd25519()
        {
            this.KeyType = "ed25519";
            return this;
        }
        public Seed SetNodeKey()
        {
            IsNodeKey = true;
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

        public IKeyPair KeyPair(int pairIndex)
        {
            if (KeyType == "ed25519")
            {
                if (pairIndex != 0)
                {
                    throw new ArgumentException("`account` is ignored for ed25519");
                }
                return EDKeyPair.From128Seed(SeedBytes);
            }
            else
            {
                pairIndex = IsNodeKey ? -1 : 0;
                var pair = K256KeyGenerator.From128Seed(SeedBytes, pairIndex);
                if (IsNodeKey)
                {
                    pair.SetNodeKey();
                }
                return pair;
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

        public static Seed FromRandom()
        {
            var rng = RandomNumberGenerator.Create();
            var seed = new byte[16];
            rng.GetBytes(seed);
            return new Seed(seed);
        }

        private static byte[] PassPhraseToSeedBytes(string phrase)
        {
            var phraseBytes = System.Text.Encoding.UTF8.GetBytes(phrase.ToCharArray());
            return (new Sha512(phraseBytes).Finish128());
        }
    }
}