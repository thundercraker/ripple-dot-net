using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ripple.TxSigning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ripple.Signing;

namespace Ripple.TxSigning.Tests
{
    [TestClass()]
    public class TxSignerTests
    {
        public const string ExpectedTxBlob =
                "12000022800000002400000001614000" +
                "0000000003E868400000000000000A73" +
                "21EDD3993CDC6647896C455F136648B7" +
                "750723B011475547AF60691AA3D7438E" +
                "021D7440C3646313B08EED6AF4392261" +
                "A31B961F10C66CB733DB7F6CD9EAB079" +
                "857834C8B0334270A2C037E63CDCCC19" +
                "32E0832882B7B7066ECD2FAEDEB4A83D" +
                "F8AE63038114C0A5ABEF242802EFED4B" +
                "041E8F2D4A8CC86AE3D18314B5F76279" +
                "8A53D543A014CAF8B297CFF8F2F937E8";

        public const string ExpectedSigningPubKey = "EDD3993CDC6647896C455F136648B7750" +
                                             "723B011475547AF60691AA3D7438E021D";

        public const string ExpectedTxnSignature = "C3646313B08EED6AF4392261A31B961F" +
                                          "10C66CB733DB7F6CD9EAB079857834C8" +
                                          "B0334270A2C037E63CDCCC1932E08328" +
                                          "82B7B7066ECD2FAEDEB4A83DF8AE6303";

        public const string ExpectedHash = "A8A9C869671D35A18DFB69AFB7741062" +
                                           "DF43F73C8A5942AD94EE58ED31477AC6";

        public const string UnsignedTxJson = @"{
            'Account': 'rJZdUusLDtY9NEsGea7ijqhVrXv98rYBYN',
            'Amount': '1000',
            'Destination': 'rHb9CJAWyB4rj91VRWn96DkukG4bwdtyTh',
            'Fee': '10',
            'Flags': 2147483648,
            'Sequence': 1,
            'TransactionType' : 'Payment'
        }";

        public const string Secret = "sEd7rBGm5kxzauRTAV2hbsNz7N45X91";

        [TestMethod()]
        public void SignTest()
        {
            var unsigned = JObject.Parse(UnsignedTxJson);
            var signed = TxSigner.Sign(unsigned, Secret);

            Assert.AreEqual(ExpectedTxnSignature, signed.TxJson["TxnSignature"]);
            Assert.AreEqual(ExpectedSigningPubKey, signed.TxJson["SigningPubKey"]);
            Assert.AreEqual(ExpectedHash, signed.Hash);
            Assert.AreEqual(ExpectedTxBlob, signed.TxBlob);
        }
    }
}