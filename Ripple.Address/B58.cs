using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace Ripple.Address
{
    internal class HashUtils
    {
        internal static byte[] DoubleDigest(byte[] buffer)
        {
            return Sha256(Sha256(buffer));
        }
        internal static byte[] Sha256(byte[] buffer)
        {
            var hash = SHA256.Create();
            return hash.ComputeHash(buffer, 0, buffer.Length);
        }
    }

    public class B58
	{
		public class Decoded
		{
			public readonly byte[] Version;
			public readonly byte[] Payload;
            public readonly string Type;

			public Decoded(byte[] version, byte[] payload, string typeName)
			{
				this.Version = version;
				this.Payload = payload;
                this.Type = typeName;
			}
		}

		private int[] _mIndexes;
		private char[] _mAlphabet;

		public B58(string alphabet)
		{
			SetAlphabet(alphabet);
			BuildIndexes();
		}

		public virtual byte[] FindPrefix(int payLoadLength, string desiredPrefix)
		{
			int totalLength = payLoadLength + 4; // for the checksum
			double chars = Math.Log(Math.Pow(256, totalLength)) / Math.Log(58);
			int requiredChars = (int) Math.Ceiling(chars + 0.2D);
			// Mess with this to see stability tests fail
			int charPos = (_mAlphabet.Length / 2) - 1;
			char padding = _mAlphabet[(charPos)];
			string template = desiredPrefix + Repeat(requiredChars, padding);
			byte[] decoded = Decode(template);
			return CopyOfRange(decoded, 0, decoded.Length - totalLength);
		}

		private static string Repeat(int times, char repeated)
		{
			char[] chars = new char[times];
            for (int i = 0; i < times; i++)
            {
                chars[i] = repeated;
            }
			return new string(chars);
		}

		private void SetAlphabet(string alphabet)
		{
			_mAlphabet = alphabet.ToCharArray();
		}

		private void BuildIndexes()
		{
			_mIndexes = new int[128];

			for (int i = 0; i < _mIndexes.Length; i++)
			{
				_mIndexes[i] = -1;
			}
			for (int i = 0; i < _mAlphabet.Length; i++)
			{
				_mIndexes[_mAlphabet[i]] = i;
			}
		}

		public virtual string EncodeToStringChecked(byte[] input, int version)
		{
			return EncodeToStringChecked(input, new byte[]{(byte) version});
		}
		public virtual string EncodeToStringChecked(byte[] input, byte[] version)
		{
		    return Encoding.ASCII.GetString(EncodeToBytesChecked(input, version));
		}

		public virtual byte[] EncodeToBytesChecked(byte[] input, int version)
		{
			return EncodeToBytesChecked(input, new byte[]{(byte) version});
		}

		public virtual byte[] EncodeToBytesChecked(byte[] input, byte[] version)
		{
			byte[] buffer = new byte[input.Length + version.Length];
			Array.Copy(version, 0, buffer, 0, version.Length);
			Array.Copy(input, 0, buffer, version.Length, input.Length);
			byte[] checkSum = CopyOfRange(HashUtils.DoubleDigest(buffer), 0, 4);
			byte[] output = new byte[buffer.Length + checkSum.Length];
			Array.Copy(buffer, 0, output, 0, buffer.Length);
			Array.Copy(checkSum, 0, output, buffer.Length, checkSum.Length);
			return EncodeToBytes(output);
		}

		public virtual string EncodeToString(byte[] input)
		{
			byte[] output = EncodeToBytes(input);
		    return Encoding.ASCII.GetString(output);
		}

		/// <summary>
		/// Encodes the given bytes in base58. No checksum is appended.
		/// </summary>
		public virtual byte[] EncodeToBytes(byte[] input)
		{
			if (input.Length == 0)
			{
				return new byte[0];
			}
			input = CopyOfRange(input, 0, input.Length);
			// Count leading zeroes.
			int zeroCount = 0;
			while (zeroCount < input.Length && input[zeroCount] == 0)
			{
				++zeroCount;
			}
			// The actual encoding.
			byte[] temp = new byte[input.Length * 2];
			int j = temp.Length;

			int startAt = zeroCount;
			while (startAt < input.Length)
			{
				byte mod = DivMod58(input, startAt);
				if (input[startAt] == 0)
				{
					++startAt;
				}
				temp[--j] = (byte) _mAlphabet[mod];
			}

			// Strip extra '1' if there are some after decoding.
			while (j < temp.Length && temp[j] == _mAlphabet[0])
			{
				++j;
			}
			// Add as many leading '1' as there were leading zeros.
			while (--zeroCount >= 0)
			{
				temp[--j] = (byte) _mAlphabet[0];
			}

			byte[] output;
			output = CopyOfRange(temp, j, temp.Length);
			return output;
		}

		public virtual byte[] Decode(string input)
		{
			if (input.Length == 0)
			{
				return new byte[0];
			}
			byte[] input58 = new byte[input.Length];
			// Transform the String to a base58 byte sequence
			for (int i = 0; i < input.Length; ++i)
			{
				char c = input[i];

				int digit58 = -1;
				if (c >= 0 && c < 128)
				{
					digit58 = _mIndexes[c];
				}
				if (digit58 < 0)
				{
					throw new EncodingFormatException("Illegal character " + c + " at " + i);
				}

				input58[i] = (byte) digit58;
			}
			// Count leading zeroes
			int zeroCount = 0;
			while (zeroCount < input58.Length && input58[zeroCount] == 0)
			{
				++zeroCount;
			}
			// The encoding
			byte[] temp = new byte[input.Length];
			int j = temp.Length;

			int startAt = zeroCount;
			while (startAt < input58.Length)
			{
				byte mod = DivMod256(input58, startAt);
				if (input58[startAt] == 0)
				{
					++startAt;
				}

				temp[--j] = mod;
			}
			// Do no add extra leading zeroes, move j to first non null byte.
			while (j < temp.Length && temp[j] == 0)
			{
				++j;
			}

			return CopyOfRange(temp, j - zeroCount, temp.Length);
		}

		/// <summary>
		/// Uses the checksum in the last 4 bytes of the decoded data to verify the rest are correct. The checksum is
		/// removed from the returned data.
		/// </summary>
		/// <exception cref="EncodingFormatException"> if the input is not baseFields 58 or the checksum does not validate. </exception>
		public virtual byte[] DecodeChecked(string input, int version)
		{
			byte[] buffer = DecodeAndCheck(input);

			byte actualVersion = buffer[0];
			if (actualVersion != version)
			{
				throw new EncodingFormatException("Bro, version is wrong yo");
			}


			return CopyOfRange(buffer, 1, buffer.Length - 4);
		}

        public virtual byte[] DecodeChecked(string input, int expectedLength, byte[] version)
        {
            return DecodeMulti(input, expectedLength, new byte[][] { version }).Payload;
        }

		public virtual Decoded DecodeMulti(string input, int expectedLength, byte[][] possibleVersions, params String[] typeNames)
		{

			byte[] buffer = DecodeAndCheck(input);
			int versionLength = buffer.Length - 4 - expectedLength;
			byte[] versionBytes = CopyOfRange(buffer, 0, versionLength);

			byte[] foundVersion = null;
            int i = 0;
            string typeName = "";
			foreach (byte[] possible in possibleVersions)
			{
				if (ArrayEquals(possible, versionBytes))
				{
					foundVersion = possible;
                    if (typeNames.Length > i)
                    {
                        typeName = typeNames[i];
                    }
					break;
				}
                i++;
			}
			if (foundVersion == null)
			{
				throw new EncodingFormatException("Bro, version is wrong yo");
			}
			byte[] bytes = CopyOfRange(buffer, versionLength, buffer.Length - 4);
			return new Decoded(foundVersion, bytes, typeName);
		}

        private bool ArrayEquals(byte[] a, byte[] b)
        {
            if (a.Length == b.Length)
            {
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] != b[i]) 
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private byte[] DecodeAndCheck(string input)
		{
			byte[] buffer = Decode(input);
			if (buffer.Length < 4)
			{
				throw new EncodingFormatException("Input too short");
			}

			byte[] toHash = CopyOfRange(buffer, 0, buffer.Length - 4);
			byte[] hashed = CopyOfRange(HashUtils.DoubleDigest(toHash), 0, 4);
			byte[] checksum = CopyOfRange(buffer, buffer.Length - 4, buffer.Length);

			if (!ArrayEquals(checksum, hashed))
			{
				throw new EncodingFormatException("Checksum does not validate");
			}
			return buffer;
		}

		//
		// number -> number / 58, returns number % 58
		//
		private byte DivMod58(byte[] number, int startAt)
		{
			int remainder = 0;
			for (int i = startAt; i < number.Length; i++)
			{
				int digit256 = (int) number[i] & 0xFF;
				int temp = remainder * 256 + digit256;

				number[i] = (byte)(temp / 58);

				remainder = temp % 58;
			}

			return (byte) remainder;
		}

		//
		// number -> number / 256, returns number % 256
		//
		private byte DivMod256(byte[] number58, int startAt)
		{
			int remainder = 0;
			for (int i = startAt; i < number58.Length; i++)
			{
				int digit58 = (int) number58[i] & 0xFF;
				int temp = remainder * 58 + digit58;

				number58[i] = unchecked((byte)(temp / 256));

				remainder = temp % 256;
			}

			return (byte) remainder;
		}

		private byte[] CopyOfRange(byte[] source, int from, int to)
		{
			byte[] range = new byte[to - from];
			Array.Copy(source, from, range, 0, range.Length);

			return range;
		}
	}

    [Serializable]
    public class EncodingFormatException : Exception
    {
        public EncodingFormatException()
        {
        }

        public EncodingFormatException(string message) : base(message)
        {
        }

        public EncodingFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EncodingFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}