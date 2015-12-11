using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ripple.Signing.Tests
{
    using Seed = Seed;

    [TestClass()]
    public class DerivationTests
    {
        [TestMethod()]
        public void GenerateRootAccountTest()
        {
            var passphrase = "masterpassphrase";
            var encodedSeed = "snoPBrXtMeMyMHUVTgbuqAfg1SUTb";
            var rootAccount = "rHb9CJAWyB4rj91VRWn96DkukG4bwdtyTh";
            var seed = Seed.FromPassPhrase(passphrase);
            var pair = seed.KeyPair();
            Assert.AreEqual(encodedSeed, seed.ToString());
            Assert.AreEqual(rootAccount, pair.Id());
        }

        [TestMethod()]
        public void GenerateNiqEd25519Test()
        {
            var passphrase = "niq";
            var encodedSeed = "sEd7rBGm5kxzauRTAV2hbsNz7N45X91";
            var accountID = "rJZdUusLDtY9NEsGea7ijqhVrXv98rYBYN";

            var idFromSeed = Seed.FromBase58(encodedSeed).KeyPair().Id();
            var seed = Seed.FromPassPhrase(passphrase).SetEd25519();
            var pair = seed.KeyPair();

            Assert.AreEqual(accountID, pair.Id());
            Assert.AreEqual(encodedSeed, seed.ToString());
            Assert.AreEqual(accountID, idFromSeed);
        }

        [TestMethod()]
        public void GenerateNodeKeyTest()
        {
            var zeroBytes = new byte[16];
            var pair = new Seed(zeroBytes).SetNodeKey().KeyPair();
            Assert.AreEqual("n9LPxYzbDpWBZ1bC3J3Fdkgqoa3FEhVKCnS8yKp7RFQFwuvd8Q2c", 
                            pair.Id());
        }
    }
}

