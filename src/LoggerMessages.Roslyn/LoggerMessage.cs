using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LoggerMessages.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace LoggerMessages.Roslyn
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

        public ExpressionStatementSyntax GetMethodCallExpression(string loggerVariable)
        {
            return SF.ExpressionStatement(
                SF.InvocationExpression(
                        SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SF.IdentifierName(loggerVariable), SF.IdentifierName(GetMethodName()))
                            .WithOperatorToken(SF.Token(SyntaxKind.DotToken)))
                    .WithArgumentList(SF.ArgumentList(
                            SF.SingletonSeparatedList<ArgumentSyntax>(
                                SF.Argument(
                                    SF.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SF.Literal(SF.TriviaList(), @"""A""", @"""A""", SF.TriviaList())))))
                        .WithOpenParenToken(SF.Token(SyntaxKind.OpenParenToken))
                        .WithCloseParenToken(SF.Token(SyntaxKind.CloseParenToken))));
        }
    }
}
