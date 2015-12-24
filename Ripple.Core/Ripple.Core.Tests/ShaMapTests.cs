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

        public TransactionResult(StObject txn, StObject meta)
        {
            _txn = txn;
            _meta = meta;
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
            return new TransactionResult(obj, obj["metaData"]);
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
        [TestMethod]
        public void EmptyMapHasZeroHash()
        {
            var shamap = new ShaMap.ShaMap();
            Assert.AreEqual(Hash256.Zero, shamap.Hash());
        }

        [TestMethod]
        public void LedgerFull38129Test()
        {
            var ledger = (JObject) Utils.ParseJsonBytes(Resources.LedgerFull38129);
            TestLedgerTreeHashing(ledger);
        }

        [TestMethod]
        public void LedgerFull40000Test()
        {
            var ledger = (JObject)Utils.ParseJsonBytes(Resources.LedgerFull40000);
            TestLedgerTreeHashing(ledger);
        }

        [TestMethod]
        public void LedgerFromFileTest()
        {
            const string ledgerJson = @"Z:\windowsshare\ledger-full-1000000.json";
            if (!File.Exists(ledgerJson)) return;
            var ledger1E6 = Utils.FileToByteArray(ledgerJson);
            var ledger = (JObject)Utils.ParseJsonBytes(ledger1E6);
            TestLedgerTreeHashing(ledger);
        }

        [TestMethod]
        public void AccountStateTest()
        {
            const string path = @"Z:\windowsshare\as-ledger-4320278.json";
            JArray state;
            if (!ParseJsonArray(path, out state)) return;
            var stateMap = ParseAccountState(state);
            Assert.AreEqual("CF37E77AE0C3BE12369133B0CA212CE7B0FCB282F5B3F1079739B69944FB0D2E", 
                stateMap.Hash().ToString());
        }

        private static bool ParseJsonArray(string path, out JArray state)
        {
            state = null;
            if (!File.Exists(path))
            {
                return false;
            }
            var ledger1E6 = Utils.FileToByteArray(path);
            state = (JArray) Utils.ParseJsonBytes(ledger1E6);
            return true;
        }

        private static ShaMap.ShaMap ParseAccountState(JArray state)
        {
            var stateMap = new ShaMap.ShaMap();
            var entries = state.Select((t) =>
            {
                StObject so = t["json"];
                Assert.AreEqual(t["binary"].ToString(), so.ToHex(), t.ToString() + " " + so.ToJson());
                return new LedgerEntry(so);
            });
            foreach (var ledgerEntry in entries)
            {
                stateMap.AddItem(ledgerEntry.Index(), ledgerEntry);
            }
            return stateMap;
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