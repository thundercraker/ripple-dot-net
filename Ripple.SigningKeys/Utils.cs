using System;
using Org.BouncyCastle.Math;
using static Org.BouncyCastle.Utilities.Encoders.Hex;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Digests;

namespace Ripple.Utils
{
    internal class Sha512
    {
        private Sha512Digest MessageDigest;

        public Sha512()
        {
            MessageDigest = new Sha512Digest();
        }

        public Sha512(byte[] start) : this()
        {
            Add(start);
        }

        public Sha512 Add(byte[] bytes)
        {
            MessageDigest.BlockUpdate(bytes, 0, bytes.Length);
            return this;
        }

        public Sha512 AddU32(uint i)
        {
            MessageDigest.Update((byte)(i >> 24 & 0xFFu));
            MessageDigest.Update((byte)(i >> 16 & 0xFFu));
            MessageDigest.Update((byte)(i >> 8 & 0xFFu));
            MessageDigest.Update((byte)(i & 0xFFu));
            return this;
        }

        private byte[] FinishTaking(int size)
        {
            byte[] finished = Finish();

            byte[] hash = new byte[size];
            Array.Copy(finished, 0, hash, 0, size);
            return hash;
        }

        public byte[] Finish()
        {
            byte[] finished = new byte[64];
            MessageDigest.DoFinal(finished, 0);
            return finished;
        }

        public byte[] Finish128()
        {
            return FinishTaking(16);
        }

        public byte[] Finish256()
        {
            return FinishTaking(32);
        }
    }

    internal class Utils
    {
        public static BigInteger UBigInt(byte[] bytes)
        {
            return new BigInteger(1, bytes);
        }

        public static string BigHex(BigInteger pub)
        {
            return ToHexString(pub.ToByteArrayUnsigned()).ToUpper();
        }

        public static string ToHex(byte[] bytes)
        {
            return ToHexString(bytes).ToUpper();
        }
    }

    internal class HashUtils
    {
        public static byte[] HalfSha512(byte[] message)
        {
            return new Sha512(message).Finish256();
        }
        public static byte[] PublicKeyHash(byte[] bytes)
        {
            var hash = SHA256Managed.Create();
            var riper = RIPEMD160Managed.Create();
            bytes = hash.ComputeHash(bytes, 0, bytes.Length);
            return riper.ComputeHash(bytes, 0, bytes.Length);
        }
    }

}
