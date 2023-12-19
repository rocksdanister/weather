using System;

namespace Drizzle.Common.Exceptions
{
    public class UnspecifiedException : Exception
    {
        public UnspecifiedException()
        {

        }

        public UnspecifiedException(string message)
          : base(message)
        {
        }

        public UnspecifiedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
