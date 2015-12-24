using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Ripple.Core.Util;

namespace Ripple.Core.Types
{
    public class AmountValue
    {
        public readonly ulong Mantissa;
        public readonly int Exponent;
        public readonly bool IsNegative;
        public readonly int Precision;
        public bool IsZero => Mantissa == 0;

        public const int MinExponent = -96;
        public const int MaxExponent = 80;
        public const int MaxPrecision = 16;

        private const string ValueRegex = @"^([-+])?(\d+)?(\.(\d+))?([eE]([+-]?\d+))?$";
        private static readonly ulong MinMantissa = Ul("1000,0000,0000,0000");
        private static readonly ulong MaxMantissa = Ul("9999,9999,9999,9999");

        private const string IllegalOfferMantissaString = "1000000000000000100";
        public static readonly AmountValue IllegalOffer = new AmountValue(
                mantissa: ulong.Parse(IllegalOfferMantissaString),
                exponent:0,
                isNegative:false,
                precision:17,
                normalise:false);

        public AmountValue(ulong mantissa,
                           int exponent,
                           bool isNegative,
                           int? precision=null,
                           bool normalise=true)
        {
            Mantissa = mantissa;
            Exponent = exponent;
            if (normalise)
            {
                Normalize(ref Mantissa, ref Exponent);
            }
            IsNegative = isNegative;
            Precision = precision ?? (IsZero ? 1 :
                                      Mantissa.ToString()
                                              .Trim('0')
                                              .Length);

        }

        public AmountValue(byte[] mantissa, int sign, int exponent=0) :
                this(ParseMantissa(mantissa), exponent, sign == -1) {}

        private static ulong ParseMantissa(byte[] mantissa)
        {
            if (mantissa.Length > 8)
            {
                throw new PrecisionError(
                    "encoded mantissa must be only 8 bytes maximum");
            }
            return Bits.ToUInt64(mantissa, 0);
        }

        public override string ToString()
        {
            if (this == IllegalOffer)
            {
                return IllegalOfferMantissaString;
            }

            if (Mantissa == 0)
            {
                return "0";
            }
            var e = 16 - Precision + Exponent;
            var str = Mantissa.ToString().Substring(0, Precision);
            while (e > 0)
            {
                str += 0;
                e--;
            }
            var decimalPos = 0;
            while (e <= 0)
            {
                decimalPos++;
                e++;
            }
            while (decimalPos > str.Length)
            {
                str = "0" + str;
            }
            if (decimalPos != 0)
            {
                var startIndex = str.Length + 1 - decimalPos;
                if (startIndex != str.Length)
                {
                    str = str.Insert(startIndex, ".");
                }
            }
            if (IsNegative)
            {
                str = "-" + str;
            }
            return str;
        }

        public static AmountValue FromString(string value)
        {
            var match = Regex.Match(value, ValueRegex);

            if (!match.Success)
            {
                throw new InvalidAmountValue($"invalid value: {value}, " +
                                             $"must match {ValueRegex}");
            }

            var signGroup = match.Groups[1];
            var numberGroup = match.Groups[2];
            var fractionGroup = match.Groups[4];
            var exponentGroup = match.Groups[6];

            var exponent = 0;
            var mantissaString = numberGroup.Success ? numberGroup.Value : "";
            if (fractionGroup.Success)
            {
                var fraction = fractionGroup.Value;
                mantissaString += fraction;
                exponent -= fraction.Length;
            }
            if (exponentGroup.Success)
            {
                exponent += int.Parse(exponentGroup.Value);
            }

            mantissaString = mantissaString.TrimStart('0');
            var trimmed = mantissaString.TrimEnd('0');
            if (trimmed.Length == 0)
            {
                trimmed = "0";
                mantissaString = "0";
            }
            exponent += mantissaString.Length - trimmed.Length;

            var mantissa = ulong.Parse(trimmed);
            var precision = trimmed.Length;
            var isNegative = signGroup.Success && signGroup.Value == "-";

            if (precision > MaxPrecision)
            {
                if (value == IllegalOfferMantissaString)
                {
                    return IllegalOffer;
                }
                throw new PrecisionError();
            }

            return new AmountValue(mantissa, exponent, isNegative, precision);
        }

        private static void Normalize(ref ulong mantissa, ref int exponent)
        {
            if (mantissa == 0) return;
            while (mantissa < MinMantissa)
            {
                mantissa *= 10;
                exponent -= 1;
            }
            while (mantissa > MaxMantissa)
            {
                mantissa /= 10;
                exponent += 1;
            }
            if (exponent > MaxExponent || exponent < MinExponent)
            {
                throw new PrecisionError();
            }
        }

        private static ulong Ul(string s)
        {
            return ulong.Parse(s.Replace(",", ""));
        }

        public static implicit operator AmountValue(JToken token)
        {
            return FromString(token.ToString());
        }
    }

    public class InvalidAmountValue : Exception
    {
        public InvalidAmountValue(string s) : base(s)
        {
        }
    }

    public class PrecisionError : Exception
    {
        public PrecisionError()
        {
        }

        public PrecisionError(string message) : base(message)
        {
        }
    }
}
