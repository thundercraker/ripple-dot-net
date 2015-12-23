using System;
using Ripple.Core.Enums;

namespace Ripple.Core.Binary
{
    public class BinarySerializer : IBytesSink
    {
        private readonly IBytesSink _sink;

        public BinarySerializer(IBytesSink sink)
        {
            _sink = sink;
        }

        public virtual void Add(byte[] n)
        {
            _sink.Add(n);
        }

        public virtual void AddLengthEncoded(byte[] n)
        {
            Add(EncodeVl(n.Length));
            Add(n);
        }

        public static byte[] EncodeVl(int length)
        {
            // TODO: bytes
            var lenBytes = new byte[4];

            if (length <= 192)
            {
                lenBytes[0] = (byte)length;
                return TakeSome(lenBytes, 1);
            }
            if (length <= 12480)
            {
                length -= 193;
                lenBytes[0] = (byte)(193u + (length >> 8));
                lenBytes[1] = (byte)(length & 0xff);
                return TakeSome(lenBytes, 2);
            }
            if (length <= 918745)
            {
                length -= 12481;
                lenBytes[0] = (byte)(241u + (length >> 16));
                lenBytes[1] = (byte)((length >> 8) & 0xff);
                lenBytes[2] = (byte)(length & 0xff);
                return TakeSome(lenBytes, 3);
            }
            throw new Exception("Overflow error");
        }

        private static byte[] TakeSome(byte[] buffer, int n)
        {
            var ret = new byte[n];
            Array.Copy(buffer, 0, ret, 0, n);
            return ret;
        }

        public virtual void Add(BytesList bl)
        {
            foreach (byte[] bytes in bl.RawList())
            {
                _sink.Add(bytes);
            }
        }

        public virtual int AddFieldHeader(Field f)
        {
            if (!f.IsSerialised)
            {
                throw new System.InvalidOperationException(
                    $"Field {f} is a discardable field");
            }
            var n = f.Header;
            Add(n);
            return n.Length;
        }

        public virtual void Add(byte type)
        {
            _sink.Add(type);
        }

        public virtual void AddLengthEncoded(BytesList bytes)
        {
            Add(EncodeVl(bytes.BytesLength()));
            Add(bytes);
        }

        public virtual void Add(Field field, ISerializedType value)
        {
            AddFieldHeader(field);
            if (field.IsVlEncoded)
            {
                AddLengthEncoded(value);
            }
            else
            {
                value.ToBytes(_sink);
                if (field.Type == FieldType.StObject)
                {
                    AddFieldHeader(Field.ObjectEndMarker);
                }
                else if (field.Type == FieldType.StArray)
                {
                    AddFieldHeader(Field.ArrayEndMarker);
                }
            }
        }

        public virtual void AddLengthEncoded(ISerializedType value)
        {
            var bytes = new BytesList();
            value.ToBytes(bytes);
            AddLengthEncoded(bytes);
        }
    }
}