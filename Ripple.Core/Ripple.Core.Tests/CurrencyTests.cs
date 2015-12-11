using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ripple.Core.Tests
{
    [TestClass()]
    public class CurrencyTests
    {
        [TestMethod()]
        public void UsdTest()
        {
            Currency c = "USD";
            Assert.IsFalse(c.IsNative);
            Assert.AreEqual("USD", c.IsoCode);
        }
    }
}