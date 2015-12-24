using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Ripple.Core.Binary;
using Ripple.Core.Enums;
using Ripple.Core.Hashing;
using Ripple.Core.ShaMap;
using Ripple.Core.Tests.Properties;
using Ripple.Core.Types;
using static System.Text.Encoding;


namespace Ripple.Core.Tests
{

    public class TransactionResult : IShaMapItem<TransactionResult>
    {
        private readonly StObject _txn;
        private readonly StObject _meta;
        private readonly int _ledgerIndex;

        public TransactionResult(StObject txn, StObject meta, int ledgerIndex)
        {
            _txn = txn;
            _meta = meta;
            _ledgerIndex = ledgerIndex;
        }

        public void ToBytesSink(IBytesSink sink)
        {
            var ser = new BinarySerializer(sink);
            ser.AddLengthEncoded(_txn);
            ser.AddLengthEncoded(_meta);
        }

        public IShaMapItem<TransactionResult> Copy()
        {
            return this;
        }

        public TransactionResult Value()
        {
            return this;
        }

        public HashPrefix Prefix()
        {
            return HashPrefix.TxNode;
        }

        public static TransactionResult FromJson(JToken obj)
        {
            return new TransactionResult(obj, obj["metaData"], 0);
        }

        public Hash256 Hash()
        {
            return (Hash256) _txn[Field.hash];
        }
    }

    public class LedgerEntry : IShaMapItem<LedgerEntry>
    {
        private readonly StObject _so;

        public LedgerEntry(StObject so)
        {
            this._so = so;
        }

        public void ToBytesSink(IBytesSink sink)
        {
            _so.ToBytes(sink);
        }

        public IShaMapItem<LedgerEntry> Copy()
        {
            return this;
        }

        public LedgerEntry Value()
        {
            return this;
        }

        public HashPrefix Prefix()
        {
            return HashPrefix.LeafNode;
        }

        public Hash256 Index()
        {
            return (Hash256) _so[Field.index];
        }
    }

    [TestClass]
    public class ShaMapTests
    {
        public static JToken ParseJsonBytes(byte[] testBytes)
        {
            var utf8 = UTF8.GetString(testBytes);
            var obj = JToken.Parse(utf8);
            return obj;
        }

        [TestMethod]
        public void EmptyMapHasZeroHash()
        {
            var shamap = new ShaMap.ShaMap();
            Assert.AreEqual(Hash256.Zero, shamap.Hash());
        }

        [TestMethod]
        public void LedgerFull38129Test()
        {
            var ledger = (JObject) ParseJsonBytes(Resources.LedgerFull38129);
            TestLedgerTreeHashing(ledger);
        }

        [TestMethod]
        public void LedgerFull40000Test()
        {
            var ledger = (JObject)ParseJsonBytes(Resources.LedgerFull40000);
            TestLedgerTreeHashing(ledger);
        }

        [TestMethod]
        public void LedgerFromFileTest()
        {
            const string ledgerJson = @"Z:\windowsshare\ledger-full-1000000.json";
            if (!File.Exists(ledgerJson)) return;
            var ledger1E6 = FileToByteArray(ledgerJson);
            var ledger = (JObject)ParseJsonBytes(ledger1E6);
            TestLedgerTreeHashing(ledger);
        }

        public byte[] FileToByteArray(string fileName)
        {
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var br = new BinaryReader(fs);
            var numBytes = new FileInfo(fileName).Length;
            var buff = br.ReadBytes((int)numBytes);
            return buff;
        }

        private static void TestLedgerTreeHashing(JObject ledger)
        {
            var txMap = new ShaMap.ShaMap();
            var stateMap = new ShaMap.ShaMap();

            var expectedTxHash = ledger["transaction_hash"].ToString();
            var expectedStateHash = ledger["account_hash"].ToString();

            var transactions = ledger["transactions"].Select(TransactionResult.FromJson);
            var state = ledger["accountState"].Select((t) => new LedgerEntry(t));

            foreach (var tr in transactions)
            {
                txMap.AddItem(tr.Hash(), tr);
            }

            foreach (var le in state)
            {
                stateMap.AddItem(le.Index(), le);
            }

            Assert.AreEqual(expectedTxHash, txMap.Hash().ToString());
            Assert.AreEqual(expectedStateHash, stateMap.Hash().ToString());
        }
    }
}