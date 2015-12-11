using System.Globalization;
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
            if (token.Type == JTokenType.String)
            {
                return new Amount(token.ToString());
            }
            return new Amount(token["value"].ToString(),  
                              token["currency"], 
                              token["issuer"]);
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
    }
}