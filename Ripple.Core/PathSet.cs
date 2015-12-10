using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Ripple.Core
{
    public class PathHop
    {
        public static byte TypeAccount = 0x01;
        public static byte TypeCurrency = 0x10;
        public static byte TypeIssuer = 0x20;

        public readonly AccountId Account;
        public readonly AccountId Issuer;
        public readonly Currency Currency;
        public readonly int Type;

        public PathHop(AccountId account, AccountId issuer, Currency currency)
        {
            Account = account;
            Issuer = issuer;
            Currency = currency;
            Type = SynthesizeType();
        }

        public static PathHop FromJson(JToken json)
        {
            return new PathHop(json["account"], json["issuer"], json["currency"]);
        }

        public bool HasIssuer()
        {
            return Issuer != null;
        }
        public bool HasCurrency()
        {
            return Currency != null;
        }
        public bool HasAccount()
        {
            return Account != null;
        }

        public int SynthesizeType()
        {
            var type = 0;

            if (HasAccount())
            {
                type |= TypeAccount;
            }
            if (HasCurrency())
            {
                type |= TypeCurrency;
            }
            if (HasIssuer())
            {
                type |= TypeIssuer;
            }
            return type;
        }

    }
    public class Path : List<PathHop>
    {
        private Path(IEnumerable<PathHop> enumerable) : base(enumerable)
        {
        }
        public static Path FromJson(JToken json)
        {
            return new Path(json.Select(PathHop.FromJson));
        }
    }

    public class PathSet : List<Path>, ISerializedType
    {
        public static byte PathSeparatorByte = 0xFF;
        public static byte PathsetEndByte = 0x00;

        private PathSet(IEnumerable<Path> enumerable) : base(enumerable)
        {
        }

        public void ToBytes(IBytesSink buffer)
        {
            var n = 0;
            foreach (var path in this)
            {
                if (n++ != 0)
                {
                    buffer.Add(PathSeparatorByte);
                }
                foreach (var hop in path)
                {
                    buffer.Add((byte)hop.Type);
                    if (hop.HasAccount())
                    {
                        buffer.Add(hop.Account.Buffer);
                    }
                    if (hop.HasCurrency())
                    {
                        buffer.Add(hop.Currency.Buffer);
                    }
                    if (hop.HasIssuer())
                    {
                        buffer.Add(hop.Issuer.Buffer);
                    }
                }
            }
            buffer.Add(PathsetEndByte);
        }

        public static PathSet FromJson(JToken token)
        {
            return new PathSet(token.Select(Path.FromJson));
        }
    }
}