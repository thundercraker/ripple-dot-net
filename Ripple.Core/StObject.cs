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
            var d = new Dictionary<Type, FromJson>
            {
                [Type.AccountId] = AccountId.FromJson,
                [Type.StObject] = FromJson,
                [Type.StArray] = StArray.FromJson,
                [Type.Blob] = Blob.FromJson,
                [Type.Uint8] = Uint8.FromJson,
                [Type.Uint32] = Uint32.FromJson,
                [Type.Uint64] = Uint64.FromJson,
                [Type.Uint16] = Uint16.FromJson,
                [Type.Amount] = Amount.FromJson,
                [Type.Hash128] = Hash128.FromJson,
                [Type.Hash256] = Hash256.FromJson,
                [Type.Hash160] = Hash160.FromJson,
                [Type.Amount] = Amount.FromJson,
                [Type.Blob] = Blob.FromJson,
                [Type.PathSet] = PathSet.FromJson,
                [Type.Vector256] = Vector256.FromJson
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
                var st = fieldForType.FromJson(pair.Value);
                so[fieldForType] = st;
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