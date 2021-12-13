using System;
using System.Collections.Generic;
using System.Text;

namespace EventGroups.Roslyn
{
    public class Constants
    {
        public const string LoggerMessagesExtensionsFileName = "LoggerMessagesExtensions.cs";
        public const string LoggerMessagesFolderName = "Extensions";
        public const string DefaultNamespace = "LoggerMessages";
        public const string ClassName = "LoggerMessagesExtensions";
        public const string DefaultContent = @"using System;
                                               using Microsoft.Extensions.Logging;";

        public const string ILoggerTypeName = "ILogger";
        public const string ILoggerModuleName = "Microsoft.Extensions.Logging.Abstractions.dll";
        public const string ILoggerModuleNamespace = "Microsoft.Extensions.Logging";
    }
}
