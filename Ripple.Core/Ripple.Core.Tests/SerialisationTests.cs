using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ripple.Core.Tests.Properties;
using Ripple.Core.Util;
using Ripple.Signing;
using static System.Text.Encoding;

namespace Ripple.Core.Tests
{

    [TestClass()]
    public class SerialisationTests
    {
        public static readonly string MessageBytes = (
            "53545800" +
            "1200002280000000240000" +
            "00016140000000000003E868400000" +
            "000000000A7321EDD3993CDC664789" +
            "6C455F136648B7750723B011475547" +
            "AF60691AA3D7438E021D8114C0A5AB" +
            "EF242802EFED4B041E8F2D4A8CC86A" +
            "E3D18314B5F762798A53D543A014CA" +
            "F8B297CFF8F2F937E8");

        public static readonly string ExpectedSig = "C3646313B08EED6AF4392261A31B961F" +
                                                    "10C66CB733DB7F6CD9EAB079857834C8" +
                                                    "B0334270A2C037E63CDCCC1932E08328" +
                                                    "82B7B7066ECD2FAEDEB4A83DF8AE6303";

        public static readonly string TxJson = @"{
                'Account': 'rJZdUusLDtY9NEsGea7ijqhVrXv98rYBYN',
                'Amount': '1000',
                'Destination': 'rHb9CJAWyB4rj91VRWn96DkukG4bwdtyTh',
                'Fee': '10',
                'Flags': 2147483648,
                'Sequence': 1,
                'SigningPubKey': 'EDD3993CDC6647896C455F136648B7750723B011475547AF60691AA3D7438E021D',
                'TransactionType' : 'Payment'
            }";

        [TestMethod()]
        public void TransactionSigningTest()
        {
            var json = JObject.Parse(TxJson);

            var obj = StObject.FromJson(json);
            var hex = obj.ToHex();
            
            // The MessageBytes includes the HashPrefix
            Assert.AreEqual(MessageBytes.Substring(8), hex);
            Seed seed = Seed.FromPassPhrase("niq").SetEd25519();
            
            // The ed25519 Signature
            var sig = seed.KeyPair().Sign(B16.FromHex(MessageBytes));
            var expectedSig = ExpectedSig;
            Assert.AreEqual(expectedSig, B16.ToHex(sig));
        }

        private static JObject GetTestsJson()
        {
            var testBytes = Resources.DataDrivenTests;
            var utf8 = UTF8.GetString(testBytes);
            var obj = JObject.Parse(utf8);
            return obj;
        }

        [TestMethod()]
        public void DataDrivenTransactionSerialisationTest()
        {
            var obj = GetTestsJson();
            foreach (var whole in obj["whole_objects"])
            {
                StObject txn = whole["tx_json"];
                Assert.AreEqual(whole["blob_with_no_signing"], txn.ToHex());
            }
        }

        [TestMethod()]
        public void DataDrivenAmountSerialisationTest()
        {
            var obj = GetTestsJson();
            Func<JToken, bool> predicate = t => t["type"].ToString() == "Amount" && 
                                                 t["expected_hex"] != null;

            var enumerable = obj["values_tests"].Where(predicate);
            var array = enumerable.ToArray();
            var passed = array.Count(test =>
            {
                var expected = test["expected_hex"].ToString();
                var fromJson = Amount.FromJson(test["test_json"]);
                var actual = fromJson.ToHex();
                var debugInfo = test["test_json"].Type + " typed: " + test.ToString();
                Assert.AreEqual(expected, actual, debugInfo);
                Assert.AreEqual(test["is_native"], fromJson.IsNative, debugInfo);
                return true;
            });
            Assert.AreEqual(array.Length, passed);
        }
    }
}