using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ripple.Core.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ripple.Core.Enums;
using Ripple.Core.Types;

namespace Ripple.Core.Tests
{
    [TestClass]
    public class TxFormatTests
    {
        [TestMethod, ExpectedException(typeof(Exception))]
        public void ValidateTest()
        {
            var so = new StObject
            {
                [Field.TransactionType] = TransactionType.Payment
            };
            TxFormat.Validate(so);
        }
    }
}