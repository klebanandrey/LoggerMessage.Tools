using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoggerMessages.Common;

namespace LoggerMessageExtension.Services
{
    public class EventGroupLocal : ILoggerGroup
    {
        public string EventGroupAbbr { get; set; }
        public string Description { get; set; }
    }
}
