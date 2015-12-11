using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Ripple.Core
{
    public class StObject : SortedDictionary<Field, ISerializedType>, ISerializedType
    {
        static StObject()
        {
            var d = new Dictionary<FieldType, FromJson>
            {
                [FieldType.AccountId] = AccountId.FromJson,
                [FieldType.StObject] = FromJson,
                [FieldType.StArray] = StArray.FromJson,
                [FieldType.Blob] = Blob.FromJson,
                [FieldType.Uint8] = Uint8.FromJson,
                [FieldType.Uint32] = Uint32.FromJson,
                [FieldType.Uint64] = Uint64.FromJson,
                [FieldType.Uint16] = Uint16.FromJson,
                [FieldType.Amount] = Amount.FromJson,
                [FieldType.Hash128] = Hash128.FromJson,
                [FieldType.Hash256] = Hash256.FromJson,
                [FieldType.Hash160] = Hash160.FromJson,
                [FieldType.Amount] = Amount.FromJson,
                [FieldType.Blob] = Blob.FromJson,
                [FieldType.PathSet] = PathSet.FromJson,
                [FieldType.Vector256] = Vector256.FromJson
            };
            foreach (var field in Field.Values.Where(
                field => d.ContainsKey(field.Type)))
            {
                field.FromJson = d[field.Type];
            }
            Field.TransactionType.FromJson = TransactionType.FromJson;
            Field.LedgerEntryType.FromJson = LedgerEntryType.FromJson;
        }

        public static StObject FromJson(JToken token)
        {
            var so = new StObject();
            foreach (var pair in (JObject) token)
            {
                var fieldForType = Field.Values[pair.Key];
                if (fieldForType != null)
                {
                    var st = fieldForType.FromJson(pair.Value);
                    so[fieldForType] = st;
                }
            }
            return so;
        }

        public void ToBytes(IBytesSink to)
        {
            ToBytes(to, null);
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
    }
}