using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ripple.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ripple.Core.Tests
{
    [TestClass()]
    public class AccountIdTests
    {
      
        [TestMethod()]
        public void ConstantsTest()
        {
            Assert.AreEqual("rrrrrrrrrrrrrrrrrrrrBZbvji", AccountId.Neutral.ToString());
            Assert.AreEqual("0000000000000000000000000000000000000001", AccountId.Neutral.ToHex());
        }
    }
}