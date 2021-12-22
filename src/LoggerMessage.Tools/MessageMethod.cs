using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LoggerMessage.Shared;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LoggerMessage.Tools
{
    public class MessageMethod
    {
        private IList<string> _parameters;

        private MessageMethod()
        {
            _parameters = new List<string>();
        }

        public string Id => $"{Abbr}{Number:D5}";

        public string Abbr { get; set; }
        public int Number { get; set; }

        public Level Level { get; set; }

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

        public IList<string> Parameters => _parameters;

        public MethodDeclarationSyntax MethodDeclaration { get; set; }

        public FieldDeclarationSyntax FieldDeclaration { get; set; }

        public ExpressionStatementSyntax ExpressionStatement { get; set; }

        public static MessageMethod Create(string Id, string template, Level level = Level.Trace, MethodDeclarationSyntax method = null, FieldDeclarationSyntax field = null, ExpressionStatementSyntax expression = null)
        {
            var message = new MessageMethod()
            {
                Level = level,
                MessageTemplate = template,
                MethodDeclaration = method,
                FieldDeclaration = field,
                ExpressionStatement = expression
            };
            if (Id.Length > 0)
            {
                message.Abbr = Id.Substring(0, Shared.Constants.AbbrLength);
                message.Number = int.Parse(Id.Substring(Shared.Constants.AbbrLength).TrimStart('0'));
            }
            return message;
        }


        private IList<string> ExtractParameters(string template)
        {
            var res = new List<string>();
            foreach (Match param in Regex.Matches(template, @"[^{\}]+(?=})"))
                res.Add(param.Value);

            return res;
        }

        public string GetMethodName(string methodIdentifier)
        {
            var methodName = _messageTemplate;

            for (var i = 0; i < _parameters.Count; i++)
            {
                methodName = methodName.Replace($"{{{_parameters[i]}}}", $"_{i}_");
            }

            methodName = methodName.Replace(" ", "_");
            while (methodName.Contains("__"))
                methodName = methodName.Replace("__", "_");

            return $"{methodIdentifier}_{Level}_{methodName}";
        }

    }
}
