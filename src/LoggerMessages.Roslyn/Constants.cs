using System;
using System.Collections.Generic;
using System.Text;

namespace EventGroups.Roslyn
{
    public class Constants
    {
        public const string LoggerMessagesFileName = "LoggerMessages.cs";
        public const string LoggerMessagesFolderName = "Extensions";
        public const string DefaultNamespace = "LoggerMessages";
        public const string ClassName = "LoggerMessagesExtensions";
        public const string DefaultContent = @"using System;
                                               using Microsoft.Extensions.Logging;";
    }
}
