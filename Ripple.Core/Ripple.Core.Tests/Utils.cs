using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Ripple.Core.Tests
{
    class Utils
    {
        public static JToken ParseJsonBytes(byte[] testBytes)
        {
            var utf8 = Encoding.UTF8.GetString(testBytes);
            var obj = JToken.Parse(utf8);
            return obj;
        }

        public static byte[] FileToByteArray(string fileName)
        {
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var br = new BinaryReader(fs);
            var numBytes = new FileInfo(fileName).Length;
            var buff = br.ReadBytes((int)numBytes);
            return buff;
        }
    }
}
