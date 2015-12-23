using System;
using Ripple.Core.Enums;
using Ripple.Core.Util;

namespace Ripple.Core.Binary
{
    public class BinaryParser
    {
        protected internal readonly int Size;
        protected internal byte[] Bytes;
        protected internal int Cursor;

        public bool End() => Cursor >= Size;
        public int Pos() => Cursor;
        public int ReadOneInt() => ReadOne() & 0xFF;

        public BinaryParser(byte[] bytes)
        {
            Size = bytes.Length;
            Bytes = bytes;
        }

        public BinaryParser(string hex) : this(B16.Decode(hex))
        {
            
        }

        public void Skip(int n) => Cursor += n;
        public byte ReadOne() => Bytes[Cursor++];

        public byte[] Read(int n)
        {
            byte[] ret = new byte[n];
            Array.Copy(Bytes, Cursor, ret, 0, n);
            Cursor += n;
            return ret;
        }

        public Field ReadField()
        {
            var fieldCode = ReadFieldCode();
            var field = Field.Values[fieldCode];
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"Couldn't parse field from " +
                          $"{fieldCode.ToString("x")}");
            }

            return field;
        }

        public int ReadFieldCode()
        {
            var tagByte = ReadOne();

            var typeBits = (tagByte & 0xFF) >> 4;
            if (typeBits == 0)
            {
                typeBits = ReadOne();
            }

            var fieldBits = tagByte & 0x0F;
            if (fieldBits == 0)
            {
                fieldBits = ReadOne();
            }

            return typeBits << 16 | fieldBits;
        }

        public int ReadVlLength()
        {
            var b1 = ReadOneInt();
            int result;

            if (b1 <= 192)
            {
                result = b1;
            }
            else if (b1 <= 240)
            {
                int b2 = ReadOneInt();
                result = 193 + (b1 - 193) * 256 + b2;
            }
            else if (b1 <= 254)
            {
                int b2 = ReadOneInt();
                int b3 = ReadOneInt();
                result = 12481 + (b1 - 241) * 65536 + b2 * 256 + b3;
            }
            else
            {
                throw new Exception(
                    "Invalid varint length indicator");
            }

            return result;
        }

        public bool End(int? customEnd)
        {
            return Cursor >= Size ||
                    (customEnd != null && Cursor >= customEnd);
        }
    }

}