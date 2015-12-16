using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using Ripple.Core.Util;

namespace Ripple.Core
{
    public class Currency : Hash160
    {
        public readonly string IsoCode;
        public readonly bool IsNative;

        public static readonly Currency Xrp = new Currency(new byte[20]);

        public Currency(byte[] buffer) : base(buffer)
        {
            IsoCode = GetCurrencyCodeFromTlcBytes(buffer, out IsNative);
        }

        public static string GetCurrencyCodeFromTlcBytes(byte[] bytes, out bool isNative)
        {
            int i;
            var zeroInNonCurrencyBytes = true;
            var allZero = true;

            for (i = 0; i < 20; i++)
            {
                allZero = allZero && bytes[i] == 0;
                zeroInNonCurrencyBytes = zeroInNonCurrencyBytes && 
                    (i == 12 || i == 13 || i == 14 || bytes[i] == 0); 
            }
            if (allZero)
            {
                isNative = true;
                return "XRP";
            }
            if (zeroInNonCurrencyBytes)
            {
                isNative = false;
                return IsoCodeFromBytesAndOffset(bytes, 12);
            }
            isNative = false;
            return null;
        }

        private static char CharFrom(byte[] bytes, int i)
        {
            return (char)bytes[i];
        }

        private static string IsoCodeFromBytesAndOffset(byte[] bytes, int offset)
        {
            var a = CharFrom(bytes, offset);
            var b = CharFrom(bytes, offset + 1);
            var c = CharFrom(bytes, offset + 2);
            return "" + a + b + c;
        }

        public new static Currency FromJson(JToken token)
        {
            return token == null ? null : FromString(token.ToString());
        }

        private static Currency FromString(string str)
        {
            if (str == "XRP")
            {
                return Xrp;
            }
            if (str.Length == 40)
            {
                return new Currency(B16.Decode(str));
            }
            if (str.Length == 3)
            {
                return new Currency(EncodeCurrency(str));
            }
            throw new InvalidOperationException();
        }

        /*
        * The following are static methods, legacy from when there was no
        * usage of Currency objects, just String with "XRP" ambiguity.
        * */
        public static byte[] EncodeCurrency(string currencyCode)
        {
            byte[] currencyBytes = new byte[20];
            currencyBytes[12] = (byte)char.ConvertToUtf32(currencyCode, 0);
            currencyBytes[13] = (byte)char.ConvertToUtf32(currencyCode, 1);
            currencyBytes[14] = (byte)char.ConvertToUtf32(currencyCode, 2);
            return currencyBytes;
        }

        public static implicit operator Currency(string v)
        {
            return FromString(v);
        }
        public static implicit operator Currency(JToken v)
        {
            return FromJson(v);
        }
        public static implicit operator JToken(Currency v)
        {
            return v.ToString();
        }

        public override string ToString()
        {
            if (IsoCode != null)
            {
                return IsoCode;
            }
            return base.ToString();
        }

        public new static Currency FromParser(BinaryParser parser, int? hint = null)
        {
            return new Currency(parser.Read(20));
        }
    }
}