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
    public class CurrencyTests
    {
        [TestMethod()]
        public void IsNativeTest()
        {
            var c = Currency.FromJson("USD");
        }
    }
}