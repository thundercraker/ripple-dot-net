using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ripple.Crypto;

namespace Ripple.Crypto.Tests
{
    using static Ripple.Address;
    using Ripple.Utils;
    using Seed = Ripple.Crypto.Seed;
    using System.IO;

    [TestClass()]
    public class DerivationTests
    {
        [TestMethod()]
        public void GenerateRootAccountTest()
        {
            string passphrase = "masterpassphrase";
            string encodedSeed = "snoPBrXtMeMyMHUVTgbuqAfg1SUTb";
            string rootAccount = "rHb9CJAWyB4rj91VRWn96DkukG4bwdtyTh";
            Seed seed = Seed.FromPassPhrase(passphrase);
            var pair = seed.KeyPair();
            Assert.AreEqual(encodedSeed, seed.ToString());
            Assert.AreEqual(rootAccount, pair.ID());
        }

        [TestMethod()]
        public void GenerateNiqEd25519Test()
        {
            string passphrase = "niq";
            string encodedSeed = "sEd7rBGm5kxzauRTAV2hbsNz7N45X91";
            string accountID = "rJZdUusLDtY9NEsGea7ijqhVrXv98rYBYN";

            var idFromSeed = Seed.FromBase58(encodedSeed).KeyPair().ID();
            Seed seed = Seed.FromPassPhrase(passphrase).SetEd25519();
            var pair = seed.KeyPair();

            Assert.AreEqual(accountID, pair.ID());
            Assert.AreEqual(encodedSeed, seed.ToString());
            Assert.AreEqual(accountID, idFromSeed);
        }

        [TestMethod()]
        public void GenerateNodeKey()
        {
            var zeroBytes = new byte[16];
            var pair = new Seed(zeroBytes).SetNodeKey().KeyPair();
            Assert.AreEqual("n9LPxYzbDpWBZ1bC3J3Fdkgqoa3FEhVKCnS8yKp7RFQFwuvd8Q2c", 
                            pair.ID());
        }
    }
}

