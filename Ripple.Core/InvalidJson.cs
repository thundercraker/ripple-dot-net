using System;

namespace Ripple.Core
{
    public class InvalidJson : Exception
    {
        public InvalidJson(string message) : base(message)
        {
        }
        public InvalidJson(string message, Exception exception) : base(message, exception)
        {

        }
    }
}