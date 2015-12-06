using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ripple.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Seed seed = Seed.FromPassPhrase(passphrase).SetEd25519();
            var pair = seed.KeyPair();
            Assert.AreEqual(accountID, pair.ID());
            Assert.AreEqual(encodedSeed, seed.ToString());
        }
    }
}