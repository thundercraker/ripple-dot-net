using System;
using Org.BouncyCastle.Math;
using static Org.BouncyCastle.Utilities.Encoders.Hex;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Digests;

namespace Ripple.Utils
{

    public class Sha512
    {
        internal Sha512Digest MessageDigest;

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

        public Sha512 AddU32(int i)
        {
            MessageDigest.Update(unchecked((byte)(((int)((uint)i >> 24)) & 0xFF)));
            MessageDigest.Update(unchecked((byte)(((int)((uint)i >> 16)) & 0xFF)));
            MessageDigest.Update(unchecked((byte)(((int)((uint)i >> 8)) & 0xFF)));
            MessageDigest.Update(unchecked((byte)((i) & 0xFF)));
            return this;
        }

        private byte[] FinishTaking(int size)
        {
            byte[] finished = Finish();

            byte[] hash = new byte[size];
            Array.Copy(finished, 0, hash, 0, size);
            return hash;
        }

        private byte[] Finish()
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

    public class Utils
    {
        internal static BigInteger UBigInt(byte[] bytes)
        {
            return new BigInteger(1, bytes);
        }

        internal static string BigHex(BigInteger _Pub)
        {
            return ToHexString(_Pub.ToByteArrayUnsigned());
        }
    }

    public class HashUtils
    {
        internal static byte[] HalfSha512(byte[] message)
        {
            return new Sha512(message).Finish256();
        }
        internal static byte[] PublicKeyHash(byte[] bytes)
        {
            var hash = SHA256Managed.Create();
            var riper = RIPEMD160Managed.Create();
            bytes = hash.ComputeHash(bytes, 0, bytes.Length);
            return riper.ComputeHash(bytes, 0, bytes.Length);
        }
    }

}
