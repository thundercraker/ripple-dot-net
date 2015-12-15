using System;

namespace Ripple.Address
{
    public class AddressCodec
    {
        public static int VerAccountId = 0;
        public static int VerFamilySeed = 33;
        public static int VerNone = 1;
        public static int VerNodePublic = 28;
        public static int VerNodePrivate = 32;
        public static int VerAccountPublic = 35;
        public static int VerAccountPrivate = 34;
        public static int VerFamilyGenerator = 41;

        public static byte[] VerK256 = {(byte)VerFamilySeed };
        public static byte[] VerEd25519 = { 0x1, 0xe1, 0x4b };
        public static byte[][] SeedVersions = {VerK256, VerEd25519};
        public static readonly string Alphabet = "rpshnaf39wBUDNEGHJKLM4PQRST7VWXYZ2bcdeCg65jkm8oFqi1tuvAxyz";

        private static readonly B58 B58;
        static AddressCodec()
        {
            B58 = new B58(Alphabet);
        }

        public static byte[] Decode(string d, int version)
        {
            return B58.DecodeChecked(d, version);
        }

        public static string Encode(byte[] d, int version)
        {
            return B58.EncodeToStringChecked(d, version);
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
            var decoded = B58.DecodeMulti(seed, 16, SeedVersions, 
                                          "secp256k1", "ed25519");
            return new DecodedSeed(decoded.Type, decoded.Payload);
        }
        public static string EncodeSeed(byte[] bytes, string type)
        {
            return type.Equals("secp256k1") ? EncodeK256Seed(bytes) : 
                                              EncodeEdSeed(bytes);
        }

        public static byte[] DecodeK256Seed(string seed)
        {
            return B58.DecodeChecked(seed, VerFamilySeed);
        }

        public static string EncodeK256Seed(byte[] bytes)
        {
            return Encode(bytes, VerFamilySeed);
        }

        public static byte[] DecodeEdSeed(string base58)
        {
            return B58.DecodeChecked(base58, 16, VerEd25519);
        }

        public static string EncodeEdSeed(byte[] bytes)
        {
            return B58.EncodeToStringChecked(bytes, VerEd25519);
        }

        public static string EncodeAddress(byte[] bytes)
        {
            return Encode(bytes, VerAccountId);
        }

        public static string EncodeNodePublic(byte[] bytes)
        {
            return Encode(bytes, VerNodePublic);
        }

        public static byte[] DecodeNodePublic(string publicKey)
        {
            return Decode(publicKey, VerNodePublic);
        }

        public static byte[] DecodeAddress(string address)
        {
            return Decode(address, VerAccountId);
        }


        public static bool IsValidAddress(string address)
        {
            try
            {
                DecodeAddress(address);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsValidNodePublic(string nodePublic)
        {
            try
            {
                DecodeNodePublic(nodePublic);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static bool IsValidSeed(string seed)
        {
            try
            {
                DecodeSeed(seed);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsValidEdSeed(string edSeed)
        {
            try
            {
                DecodeEdSeed(edSeed);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsValidK256Seed(string k256Seed)
        {
            try
            {
                DecodeK256Seed(k256Seed);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


    }
}
