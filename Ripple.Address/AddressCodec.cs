using System;

namespace Ripple.Address
{
    public class AddressCodec
    {
        public const string Alphabet = "rpshnaf39wBUDNEGHJKLM4PQRST7VWXYZ2bcdeCg65jkm8oFqi1tuvAxyz";

        public static B58.Version AccountId = MakeVersion(0, 20);
        public static B58.Version K256Seed = MakeVersion(33, 16);
        public static B58.Version NodePublic = MakeVersion(28, 33);
        public static B58.Version NodePrivate = MakeVersion(32, 32);
        public static B58.Version AccountPublic = MakeVersion(35, 33);
        public static B58.Version AccountPrivate = MakeVersion(34, 32);
        public static B58.Version FamilyGenerator = MakeVersion(41, 33);
        public static B58.Version Ed25519Seed = MakeVersion(new byte[]{ 0x1, 0xe1, 0x4b }, 16);
        public static B58.Versions AnySeed = B58.Versions
                                                .With("secp256k1", K256Seed)
                                                .And("ed25519", Ed25519Seed);

        public static B58.Version MakeVersion(byte versionByte, int expectedLength)
        {
            return MakeVersion(new []{ versionByte}, expectedLength);
        }
        public static B58.Version MakeVersion(byte[] versionBytes, int expectedLength)
        {
            return new B58.Version(versionBytes, expectedLength);
        }

        private static readonly B58 B58;
        static AddressCodec()
        {
            B58 = new B58(Alphabet);
        }

        public class DecodedSeed
        {
            public readonly string Type;
            public readonly byte[] Bytes;

            public DecodedSeed(string type, byte[] payload)
            {
                Type = type;
                Bytes = payload;
            }
        }

        public static DecodedSeed DecodeSeed(string seed)
        {
            var decoded = B58.Decode(seed, AnySeed);
            return new DecodedSeed(decoded.Type, decoded.Payload);
        }

        public static string EncodeSeed(byte[] bytes, string type)
        {
            return B58.Encode(bytes, type, AnySeed);
        }

        public static byte[] DecodeK256Seed(string seed)
        {
            return B58.Decode(seed, K256Seed);
        }

        public static string EncodeK256Seed(byte[] bytes)
        {
            return B58.Encode(bytes, K256Seed);
        }

        public static byte[] DecodeEdSeed(string base58)
        {
            return B58.Decode(base58, Ed25519Seed);
        }

        public static string EncodeEdSeed(byte[] bytes)
        {
            return B58.Encode(bytes, Ed25519Seed);
        }

        public static string EncodeAddress(byte[] bytes)
        {
            return B58.Encode(bytes, AccountId);
        }

        public static string EncodeNodePublic(byte[] bytes)
        {
            return B58.Encode(bytes, NodePublic);
        }

        public static byte[] DecodeNodePublic(string publicKey)
        {
            return B58.Decode(publicKey, NodePublic);
        }

        public static byte[] DecodeAddress(string address)
        {
            return B58.Decode(address, AccountId);
        }

        public static bool IsValidAddress(string address)
        {
            return B58.IsValid(address, AccountId);
        }

        public static bool IsValidNodePublic(string nodePublic)
        {
            return B58.IsValid(nodePublic, NodePublic);
        }

        public static bool IsValidSeed(string seed)
        {
            return B58.IsValid(seed, AnySeed);
        }

        public static bool IsValidEdSeed(string edSeed)
        {
            return B58.IsValid(edSeed, Ed25519Seed);
        }

        public static bool IsValidK256Seed(string k256Seed)
        {
            return B58.IsValid(k256Seed, K256Seed);
        }
    }
}
