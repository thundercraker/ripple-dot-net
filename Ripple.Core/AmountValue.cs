using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Ripple.Core.Util;

namespace Ripple.Core
{
    public class AmountValue
    {
        public readonly ulong Mantissa;
        public readonly int Exponent;
        public readonly bool IsNegative;
        public readonly int Precision;
        public bool IsZero => Mantissa == 0;

        private const string ValueRegex = @"([-+])?(\d+)?(.(\d+))?([eE]([+-]?\d+))?";
        private static readonly ulong MinMantissa = Ul("1000,0000,0000,0000");
        private static readonly ulong MaxMantissa = Ul("9999,9999,9999,9999");

        public AmountValue(ulong mantissa,
                           int exponent,
                           bool isNegative,
                           int? precision=null)
        {
            Mantissa = mantissa;
            Exponent = exponent;
            Normalize(ref Mantissa, ref Exponent);
            IsNegative = isNegative;
            Precision = precision ?? (IsZero ? 1 :
                                      Mantissa.ToString()
                                              .Trim('0')
                                              .Length);
        }

        public AmountValue(byte[] mantissa, int sign, int exponent=0) :
                this(Bits.ToUInt64(mantissa, 0), exponent, sign == -1) {}

        public override string ToString()
        {
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
}
