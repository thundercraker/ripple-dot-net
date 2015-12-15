using Microsoft.VisualStudio.TestTools.UnitTesting;

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