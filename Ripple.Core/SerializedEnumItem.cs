using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using Ripple.Core.Util;

namespace Ripple.Core
{
    public abstract class SerializedEnumItem<TOrd> : EnumItem, ISerializedType
        where TOrd : struct, IConvertible
    {
        protected readonly byte[] Bytes; 
        public void ToBytes(IBytesSink sink)
        {
            sink.Add(Bytes);
        }

        public JToken ToJson()
        {
            return ToString();
        }

        protected SerializedEnumItem(string name, int ordinal) : base(name, ordinal)
        {
            var width = Marshal.SizeOf(default(TOrd));
            switch (width)
            {
                case 1:
                    Bytes = new[] { (byte)ordinal };
                    break;
                case 2:
                    Bytes = Bits.GetBytes((ushort) ordinal);
                    break;
            }
        }
    }
}