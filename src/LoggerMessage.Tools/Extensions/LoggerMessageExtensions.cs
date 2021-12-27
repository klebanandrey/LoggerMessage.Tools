using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LoggerMessage.Tools.Extensions
{
    public static class LoggerMessageExtensions
    {
        public static ExpressionStatementSyntax GenerateInvocation(this MessageMethod messageMethod, string loggerVariable)
        {
            var arguments = messageMethod.Parameters.Select(p => SF.Argument(SF.IdentifierName(p)));

            return SF.ExpressionStatement(
                SF.InvocationExpression(
                        SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SF.IdentifierName(loggerVariable),
                                SF.IdentifierName(messageMethod.GetMethodName()))
                            .WithOperatorToken(SF.Token(SyntaxKind.DotToken)))
                    .WithArgumentList(SF.ArgumentList(SF.SeparatedList<ArgumentSyntax>(arguments))
                        .WithOpenParenToken(SF.Token(SyntaxKind.OpenParenToken))
                        .WithCloseParenToken(SF.Token(SyntaxKind.CloseParenToken))));
        }
    }
}
