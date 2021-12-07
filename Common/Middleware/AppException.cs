using System;
using System.Globalization;

namespace Common.Middleware
{
    public class AppException : Exception
    {
        public string UserFriendlyMessage { get; set; }

        public AppException() : base()
        {
        }

        public AppException(string message) : base(message)
        {
        }

        public AppException(string message, params object[] args)
            : base(String.Format(CultureInfo.CurrentCulture, message, args))
        {
        }
    }
}