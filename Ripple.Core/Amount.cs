using System.Security.Policy;
using Newtonsoft.Json.Linq;
using Deveel.Math;

namespace Ripple.Core
{
    using Util;
    public class Amount : ISerializedType
    {
        public readonly AccountId Issuer;
        public readonly Currency Currency;
        public readonly BigDecimal Value;
        public bool IsNative => Currency.IsNative;

        public const int MaximumIouPrecision = 16;
        public const int MaximumNativeScale = 6;

        private static MathContext _mathContext = new MathContext(16, RoundingMode.HalfUp);

        public Amount(string value = "0", 
                      Currency currency=null, 
                      AccountId issuer=null)
        {
            Value =  ParseDecimal(value);
            Currency = currency ?? Currency.Xrp;
            Issuer = issuer ?? (Currency.IsNative ?
                                    AccountId.Zero :
                                    AccountId.Neutral);
        }

        private static BigDecimal ParseDecimal(string value)
        {
            _mathContext = MathContext.Decimal64;
            return BigDecimal.Parse(value, _mathContext);
        }

        public void ToBytes(IBytesSink sink)
        {
            var notNegative = Value.Sign != -1;
            var mantissa = CalculateMantissa();

            if (IsNative)
            {
                mantissa[0] |= (byte) (notNegative ?  0x40 : 0x00);
                sink.Add(mantissa);
            }
            else
            {
                mantissa[0] |= 0x80;
                if (!Value.IsZero)
                {
                    if (notNegative)
                    {
                        mantissa[0] |= 0x40;
                    }
                    var exponent = CalculateExponent();
                    var exponentByte = 97 + exponent;
                    mantissa[0] |= (byte)(exponentByte >> 2);
                    mantissa[1] |= (byte)((exponentByte & 0x03) << 6);
                }
                sink.Add(mantissa);
                Currency.ToBytes(sink);
                Issuer.ToBytes(sink);
            }
        }

        public JToken ToJson()
        {
            if (IsNative)
            {
                return Value.ToString();
            }
            return new JObject()
            {
                ["value"] = Value.ToPlainString(),
                ["currency"] = Currency,
                ["issuer"] = Issuer,
            };
        }

        public static Amount FromJson(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Integer:
                    return (ulong)token;
                case JTokenType.String:
                    return new Amount(token.ToString());
                case JTokenType.Object:
                    return new Amount(token["value"].ToString(),
                        token["currency"],
                        token["issuer"]);
                default:
                    throw new InvalidJson("Can not create " +
                                          $"amount from `{token}`");
            }
        }

        public static implicit operator Amount(ulong a)
        {
            return new Amount(a.ToString("D"));
        }

        private byte[] CalculateMantissa()
        {
            var e = CalculateExponent();
            var scaledAbs = Value.Abs().ScaleByPowerOfTen(-e);
            var byteArray = scaledAbs
                .ToBigIntegerExact()
                .ToByteArray();
            return Utils.ZeroPad(byteArray, 8);
        }

        private int CalculateExponent()
        {
            return IsNative ? 0 :
                -MaximumIouPrecision + 
                Value.Precision -
                Value.Scale;
        }

        public static Amount FromParser(BinaryParser parser, int? hint=null)
        {
            BigDecimal value;
            var mantissa = parser.Read(8);
            var b1 = mantissa[0];
            var b2 = mantissa[1];

            var isIou = (b1 & 0x80) != 0;
            var isPositive = (b1 & 0x40) != 0;
            var sign = isPositive ? 1 : -1;

            if (isIou)
            {
                mantissa[0] = 0;
                var curr = Currency.FromParser(parser);
                var issuer = AccountId.FromParser(parser);
                var exponent = ((b1 & 0x3F) << 2) + ((b2 & 0xff) >> 6) - 97;
                mantissa[1] &= 0x3F;

                value = new BigDecimal(new BigInteger(sign, mantissa), -exponent);
                return new Amount(value.StripTrailingZeros(), curr, issuer);
            }
            mantissa[0] &= 0x3F;
            value = DropsFromMantissa(mantissa, sign);
            return new Amount(value);
        }

        private static BigDecimal DropsFromMantissa(byte[] mantissa, int sign)
        {
            return new BigDecimal(new BigInteger(sign, mantissa));
        }
    }
}