using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LoggerMessage.Shared
{
    public class LoggerMessage
    {
        private IList<string> _parameters;

        private LoggerMessage()
        {
            _parameters = new List<string>();
        }

        private string _messageTemplate;
        public string MessageTemplate
        {
            get { return _messageTemplate;}  
            set
            {
                _messageTemplate = value;
                _parameters = ExtractParameters(value);
            }
        }

        public IEventGroup Group { get; set; }

        public IList<string> Parameters => _parameters;

        public string LoggerVariable { get; set; }

        public static LoggerMessage Create(string template)
        {
            var message = new LoggerMessage();
            message.MessageTemplate = template;
            return message;
        }


        private IList<string> ExtractParameters(string template)
        {
            var res = new List<string>();
            foreach (Match param in Regex.Matches(template, @"[^{\}]+(?=})"))
                res.Add(param.Value);

            return res;
        }

        public string GetMethodSignature()
        {
            var parameters = String.Join(", ", _parameters.Select(p => $"string {p}"));

            return GetMethodName() + $"({parameters})";
        }

        public string GetMethodName()
        {
            var methodName = _messageTemplate;

            for (var i = 0; i < _parameters.Count; i++)
            {
                methodName = methodName.Replace($"{{{_parameters[i]}}}", $"_{i}_");
            }

            methodName = methodName.Replace(" ", "_");
            while (methodName.Contains("__"))
                methodName = methodName.Replace("__", "_");

            return methodName;
        }

        public string GetMethodCall(string loggerVariable)
        {
            return $"{loggerVariable}.{GetMethodSignature()}";
        }
    }
}
