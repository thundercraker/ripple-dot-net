using System;
using NUnit.Framework;
using Ripple.Testing.Utils;

namespace Ripple.Testing.NUnit3
{
    public class NUnitFixtureScopedRippledBase : FixtureScopedRippledBase
    {
        [SetUp]
        public override void PerformSetUp()
        {
            SetUpTest();
        }

        [TearDown]
        public override void PerformTearDown()
        {
            TearDownTest();
        }

        [OneTimeSetUp]
        public override void PerformOneTimeSetUp()
        {
            SetUpFixture();
        }

        [OneTimeTearDown]
        public override void PerformOneTimeTearDown()
        {
            TearDownFixture();
        }

        public override string RippledBasePath
            => Environment.GetEnvironmentVariable("RIPPLED_TEST_FOLDER") ??
               @"C:\ripple-dot-net\tests";

        public override string RippledBinaryPath
            => Environment.GetEnvironmentVariable("RIPPLED_BINARY") ??
               @"C:\rippled\build\rippled.exe";

        public override ITestFrameworkAbstraction TestHelper()
        {
            return NUnitTestFrameworkAbstraction.Impl;
        }
    }
}
