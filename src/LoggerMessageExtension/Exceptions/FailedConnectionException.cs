﻿using System;

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
