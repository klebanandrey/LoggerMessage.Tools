using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LoggerMessages.Roslyn.Extensions
{
    public static class LoggerMessageExtensions
    {
        public static ExpressionStatementSyntax GetMethodCallExpression(this LoggerMessage.Shared.LoggerMessage loggerMessage)
        {
            return SF.ExpressionStatement(
                SF.InvocationExpression(
                        SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SF.IdentifierName(loggerMessage.LoggerVariable), SF.IdentifierName(loggerMessage.GetMethodName()))
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
