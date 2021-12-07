using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestLoggerMessages
{
    class Class1
    {
    }

    class Class2
    {
        private int Property { get; set; }

        private ILogger Logger;

        private ILogger Logger1 { get; }


    }
}


namespace TestLoggerMessages1
{
    class Class12
    {
        private ILogger Logger;
    }
}
