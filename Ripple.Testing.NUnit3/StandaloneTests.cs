using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Ripple.Core.Types;

namespace Ripple.Testing.NUnit3
{
    [Timeout((int)15e3)]
    [TestFixture]
    public class StandaloneTests : NUnitFixtureScopedRippledBase
    {
        [OneTimeSetUp]
        public async Task SetupState()
        {
            await SetupWithDefaultRipple(
                MARY < Balance(5000/XRP)
                     < Line(99.991m, // balance
                            100/USD/GW1, With(NoRippleFlag)) // limit
                     < Line(100/USD/ROOT)
                     < Order(50/XRP, 5/USD/GW1, With(SellFlag))
                     < Order(50/XRP, 5/USD/MARY)
                     < Order(50/XRP, 4/USD/MARY)
                     < Order(50/XRP, 3/USD/ROOT)
               ,
                JOE < Line(105.21m, 150/USD/GW1)
            );
        }

        [Test]
        public async Task P100_BasicApi()
        {
            // Futur)istic
            await Pay(ROOT, MARY, 1e3m/XRP);
            // Expect)ations
            await Expect(Trust(MARY, 100/USD/ROOT));

            // Of Composability
            var results = await SubmitAndClose(
                Expect(Trust(MARY, 100/USD/ROOT)),
                Expect(Offer(MARY, 5000/XRP, 100/USD/MARY)),
                Expect(Offer(ROOT, 100/USD/MARY, 5000/XRP)),
                // And localised error messages
                Expect(EngineResult.temINVALID_FLAG,
                       Offer(MARY, 5000/XRP, 100/USD/MARY,
                                   With(SetFlag(0x10))))
            );
            // We have no TxResult
            Assert.IsNull(results.Last());
        }

        [Test]
        public async Task P200_AccountStateCheck()
        {
            var actual = await GetNormalisedAccountStateHash();
            var expected = Hash256.FromHex(
                "CA1CBA560D6623BE9905D966433076A07C0E8E757CF871710C488E2950E1CA88");
            Assert.AreEqual(expected, actual);
        }
    }
}
