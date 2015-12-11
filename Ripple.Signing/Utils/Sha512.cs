using System;
using Org.BouncyCastle.Crypto.Digests;

namespace Ripple.Signing.Utils
{
    public class Sha512
    {
        private readonly Sha512Digest _messageDigest;

        public Sha512()
        {
            _messageDigest = new Sha512Digest();
        }

        public Sha512(byte[] start) : this()
        {
            Add(start);
        }

        public Sha512 Add(byte[] bytes)
        {
            _messageDigest.BlockUpdate(bytes, 0, bytes.Length);
            return this;
        }

        public Sha512 AddU32(uint i)
        {
            _messageDigest.Update((byte)(i >> 24 & 0xFFu));
            _messageDigest.Update((byte)(i >> 16 & 0xFFu));
            _messageDigest.Update((byte)(i >> 8 & 0xFFu));
            _messageDigest.Update((byte)(i & 0xFFu));
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
            _messageDigest.DoFinal(finished, 0);
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
}