using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ripple.Core.Util
{
    public class B16
    {
        public static string[] HexTbl = Enumerable.Range(0, 256).Select(v => v.ToString("X2")).ToArray();
        public static string ToHex(IEnumerable<byte> array)
        {
            StringBuilder s = new StringBuilder();
            foreach (var v in array)
                s.Append(HexTbl[v]);
            return s.ToString();
        }
        public static string ToHex(byte[] array)
        {
            StringBuilder s = new StringBuilder(array.Length * 2);
            foreach (var v in array)
                s.Append(HexTbl[v]);
            return s.ToString();
        }
        public static byte[] FromHex(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}