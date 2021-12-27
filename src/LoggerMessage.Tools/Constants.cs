﻿namespace LoggerMessage.Tools
{
    public class Constants
    {
        public const string LoggerMessagesExtensionsFileName = "LoggerMessagesExtensions.cs";
        public const string LoggerMessagesFolderName = "Extensions";
        public const string DefaultNamespace = "LoggerMessages";
        public const string ClassName = "LoggerMessagesExtensions";
        public const string LoggerMessagesFileName = "LoggerMessages.cs";
        public const string LoggerMessagesResxFileName = "LoggerMessages.resx";

        public const string DefaultContent = @"using System;
                                               using Microsoft.Extensions.Logging;";

        public const string ILoggerTypeName = "ILogger";
        public const string ILoggerModuleName = "Microsoft.Extensions.Logging.Abstractions.dll";
        public const string ILoggerModuleNamespace = "Microsoft.Extensions.Logging";
        public const string LoggerVariable = "_logger";
        public const string LoggerMessagesResxFolderName = "Properties";

        public const string GeneratedCodeNotification =
            @"
//------------------------------------------------------------------------------
// <auto-generated>
// This file was generated automatically by LoggerMessage.Tools https://github.com/klebanandrey/LoggerMessage.Tools
// Don't edit this file. It can be regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
";
    }
}
