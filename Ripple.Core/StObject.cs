using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Ripple.Core
{
    public class StObject : SortedDictionary<Field, ISerializedType>, ISerializedType
    {
        internal class BuildFrom
        {
            public  FromParser Parser;
            public  FromJson Json;

            public BuildFrom(FromJson json, FromParser parser=null)
            {
                Parser = parser;
                Json = json;
            }
        }

        static StObject()
        {
            var d2 = new Dictionary<FieldType, BuildFrom>
            {
                [FieldType.StObject] = new BuildFrom(FromJson, FromParser),
                [FieldType.StArray] = new BuildFrom(StArray.FromJson, StArray.FromParser),
                [FieldType.Uint8] = new BuildFrom(Uint8.FromJson, Uint8.FromParser),
                [FieldType.Uint32] = new BuildFrom(Uint32.FromJson, Uint32.FromParser),
                [FieldType.Uint64] = new BuildFrom(Uint64.FromJson, Uint64.FromParser),
                [FieldType.Uint16] = new BuildFrom(Uint16.FromJson, Uint16.FromParser),
                [FieldType.Amount] = new BuildFrom(Amount.FromJson, Amount.FromParser),
                [FieldType.Hash128] = new BuildFrom(Hash128.FromJson, Hash128.FromParser),
                [FieldType.Hash256] = new BuildFrom(Hash256.FromJson, Hash256.FromParser),
                [FieldType.Hash160] = new BuildFrom(Hash160.FromJson, Hash160.FromParser),
                [FieldType.AccountId] = new BuildFrom(AccountId.FromJson, AccountId.FromParser),
                [FieldType.Blob] = new BuildFrom(Blob.FromJson, Blob.FromParser),
                [FieldType.PathSet] = new BuildFrom(PathSet.FromJson, PathSet.FromParser),
                [FieldType.Vector256] = new BuildFrom(Vector256.FromJson, Vector256.FromParser),
            };
            foreach (var field in Field.Values.Where(
                        field => d2.ContainsKey(field.Type)))
            {
                var buildFrom = d2[field.Type];
                field.FromJson = buildFrom.Json;
                field.FromParser= buildFrom.Parser;
            }
            Field.TransactionType.FromJson = TransactionType.FromJson;
            Field.TransactionType.FromParser= TransactionType.FromParser;
            Field.TransactionType.FromJson = TransactionType.FromJson;
            Field.LedgerEntryType.FromJson = LedgerEntryType.FromJson;
        }

        public static StObject FromParser(BinaryParser parser, int? hint = null)
        {
            var so = new StObject();

            // hint, is how many bytes to parse
            if (hint != null)
            {
                // end hint
                hint = parser.Pos() + hint;
            }

            while (!parser.End(hint))
            {
                var field = parser.ReadField();
                if (field == Field.ObjectEndMarker)
                {
                    break;
                }
                var sizeHint = field.IsVlEncoded ? parser.ReadVlLength() : (int?) null;
                var st = field.FromParser(parser, sizeHint);
                if (st == null)
                {
                    throw new InvalidOperationException("Parsed " + field + " as null");
                }
                so[field] = st;
            }
            return so;
        }

        public static StObject FromJson(JToken token)
        {
            var so = new StObject();
            foreach (var pair in (JObject) token)
            {
                if (!Field.Values.Has(pair.Key))
                {
                    continue;
                }
                var fieldForType = Field.Values[pair.Key];
                var st = fieldForType.FromJson(pair.Value);
                so[fieldForType] = st;
            }
            return so;
        }

        public void ToBytes(IBytesSink to)
        {
            ToBytes(to, null);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            foreach (var pair in this)
            {
                json[pair.Key] = pair.Value.ToJson();
            }
            return json;
        }

        public void ToBytes(IBytesSink to, Func<Field, bool> p)
        {
            var serializer = new BinarySerializer(to);
            foreach (var pair in this.Where(pair => p == null || p(pair.Key)))
            {
                serializer.Add(pair.Key, pair.Value);
            }
        }

        public static implicit operator StObject(JToken v)
        {
            return FromJson(v);
        }

        public static StObject FromHex(string s)
        {
            return FromParser(new BinaryParser(s));
        }
    }
}