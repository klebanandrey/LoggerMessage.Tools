using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggerMessageExtension.Exceptions
{
    public class FailedConnectionException : Exception
    {
        public FailedConnectionException() : base("Failed connection to logger message service")
        {}

        public FailedConnectionException(string message) : base(message)
        {}
    }
}
