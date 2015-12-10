using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Ripple.Core
{
    public class StArray : List<StObject>, ISerializedType
    {
        public StArray(IEnumerable<StObject> collection) : base(collection)
        {
        }

        public void ToBytes(IBytesSink sink)
        {
            foreach (var so in this)
            {
                so.ToBytes(sink);
            }
        }

        public static StArray FromJson(JToken token)
        {
            return new StArray(token.Select(StObject.FromJson));
        }
    }
}