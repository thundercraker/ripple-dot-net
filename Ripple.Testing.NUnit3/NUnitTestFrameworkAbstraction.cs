using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Ripple.Testing.Utils;

namespace Ripple.Testing.NUnit3
{
    public class NUnitTestFrameworkAbstraction : ITestFrameworkAbstraction

    {
        public string CurrentTestName()
        {
            return TestContext.CurrentContext.Test.Name;
        }

        public bool DidTestFail()
        {
            return TestContext.CurrentContext.Result.FailCount > 0;
        }

        public void RunAsyncAction(Func<Task> action)
        {
            AsyncContext.Run(action);
        }

        public Type TestAttributeType()
        {
            return typeof (TestAttribute);
        }

        public static readonly ITestFrameworkAbstraction Impl =
                new NUnitTestFrameworkAbstraction();
    }
}