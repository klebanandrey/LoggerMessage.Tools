using Microsoft.Extensions.Logging;

namespace TestLoggerMessages
{
    class CheckLoggerProperty
    {
        private int Property { get; set; }

        private ILogger Logger { get; }
    }
}