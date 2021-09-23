using System;
using System.Collections.Generic;
using System.Text;

namespace LoggerMessages.Common
{
    public interface ILoggerGroup
    {
        string EventGroupAbbr { get; set; }

        string Description { get; set; }
    }
}
