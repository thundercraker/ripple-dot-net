using System;
using Newtonsoft.Json.Linq;


namespace Ripple.Core
{
    using Util;
    public class Amount : ISerializedType
    {
        public readonly AccountId Issuer;
        public readonly Currency Currency;
        public bool IsNative => Currency.IsNative;

        public const int MaximumIouPrecision = 16;
        public const int MaximumNativeScale = 6;

        public AmountValue Value;

        public Amount(AmountValue value,
                      Currency currency=null,
                      AccountId issuer=null)
        {
            Currency = currency ?? Currency.Xrp;
            Issuer = issuer ?? (Currency.IsNative ?
                                    AccountId.Zero :
                                    AccountId.Neutral);
            Value = value;
        }

        public Amount(string v="0", Currency c=null, AccountId i=null) :
                      this(AmountValue.FromString(v), c, i)
        {
        }

        public void ToBytes(IBytesSink sink)
        {
            var notNegative = !Value.IsNegative;
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
                    var exponent = Exponent;
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
            return new JObject
            {
                ["value"] = Value.ToString(),
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
                    return new Amount(token);
                case JTokenType.Object:
                    return new Amount(
                        token["value"],
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
            var units = Value.Mantissa;
            if (IsNative)
            {
                // TODO, this seems kind of pointless, and the 
                // AmountValue should just store it as a mantissa
                // with an exponent of zero?
                units /= (ulong) Math.Pow(10, -Value.Exponent);
            }
            return Bits.GetBytes(units);
        }

        public int Exponent => Value.Exponent;

        public static Amount FromParser(BinaryParser parser, int? hint=null)
        {
            AmountValue value;
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
                value = new AmountValue(mantissa, sign, exponent);
                return new Amount(value, curr, issuer);
            }
            mantissa[0] &= 0x3F;
            value = new AmountValue(mantissa, sign);
            return new Amount(value);
        }
    }
}