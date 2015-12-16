using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ripple.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ripple.Core.Util;

namespace Ripple.Core.Tests
{
    [TestClass()]
    public class IouValueTests
    {
        [TestMethod()]
        public void TestScribble()
        {
            AssertValue("0000", "0", 0, false);
            AssertValue("-000.1", "1000,0000,0000,0000", -16, true);
            AssertValue("-.1", "1000,0000,0000,0000", -16, true, precision:1);
            AssertValue(".1", "1000,0000,0000,0000", -16, false, precision:1);
            AssertValue("9999,9999,9999,9999", "9999,9999,9999,9999", 0, false, precision: 16);
            AssertValue("9999,9999", "9999,9999,0000,0000", -8, false, precision: 8);
            AssertValue("99.123", "9912,3000,0000,0000", -14, false, precision: 5);
            AssertValue("0.00123", "1230,0000,0000,0000", -18, false, precision: 3);
            AssertValue("120000", "1200,0000,0000,0000", -10, false, precision:2);

            //00 80C6 A47E 8D03//
            //FF FFC0 6FF2 8623
        }

        public void AssertValue(string value, string mantissa, int exponent, bool isNegative, int? precision = null)
        {
            Normalize(ref mantissa);
            Normalize(ref value);

            var val = AmountValue.FromString(value);

            Assert.AreEqual(mantissa, val.Mantissa.ToString(), $"mantissa for {value}");
            Assert.AreEqual(exponent, val.Exponent, $"exponent for {value}");
            Assert.AreEqual(isNegative, val.IsNegative, $"isNegative for {value}");

            if (precision != null)
            {
                Assert.AreEqual(precision, val.Precision, $"precision for {value}");
            }

            Debug.WriteLine($"value: {value} val {val}  mantissaBytes: {B16.ToHex(Bits.GetBytes(val.Mantissa))}");
        }

        public static void Normalize(ref string v)
        {
            v = v.Replace(",", "");
        }
    }
}
