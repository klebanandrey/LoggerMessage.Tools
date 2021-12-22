using System;
using System.Collections.Generic;
using System.Linq;
using LoggerMessage.Shared;
using LoggerMessages.Roslyn.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Constants = EventGroups.Roslyn.Constants;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LoggerMessages.Roslyn
{
    public class LoggerExtensions
    {
        public Document Extensions { get; }

        private LoggerExtensions(Document extensions)
        {
            Extensions = extensions;
            MessageMethods = new List<MessageMethod>();
        }

        public List<MessageMethod> MessageMethods {get; set; }

        private static FieldDeclarationSyntax GetField(IEnumerable<FieldDeclarationSyntax> fields, string id)
        {
            return fields.FirstOrDefault(f =>
            {
                var variable = f.Declaration.DescendantNodes<VariableDeclaratorSyntax>().FirstOrDefault();
                return variable != null && variable.Identifier.Text.Contains(id);
            });
        }

        private static ExpressionStatementSyntax GetExpression(IEnumerable<ExpressionStatementSyntax> expressions, string id)
        {
            return expressions.FirstOrDefault(e =>
            {
                var identifierName = e.Expression.DescendantNodes<IdentifierNameSyntax>().FirstOrDefault();
                return identifierName != null && identifierName.Identifier.Text.Contains(id);
            });
        }

        private static Level GetLevel(MethodDeclarationSyntax methodDeclaration)
        {
            var messageId = methodDeclaration.Identifier.Text.Split('_')[1];
            return (Level)Enum.Parse(typeof(Level), messageId);
        }


        public static LoggerExtensions Init(Document extensionsDocument)
        {
            var extensions = new LoggerExtensions(extensionsDocument);
            var classNodes = extensionsDocument.GetClassDeclaration().DescendantNodes();

            var methods = classNodes.OfType<MethodDeclarationSyntax>();
            var fields = classNodes.OfType<FieldDeclarationSyntax>();
            var expressions = classNodes.OfType<ConstructorDeclarationSyntax>().FirstOrDefault()
                .DescendantNodes<ExpressionStatementSyntax>();

            foreach (var method in methods)
            {
                var id = method.GetIdFromMethodDeclaration();
                var field = GetField(fields, id);
                var expression = GetExpression(expressions, id);
                var level = GetLevel(method);

                extensions.MessageMethods.Add(MessageMethod.Create(id, method.GetTemplateFromMethodDeclaration(), level, method, field, expression));
            }
            return extensions;
        }

        private MethodDeclarationSyntax CreateMethodDeclaration(MessageMethod messageMethod, ClassDeclarationSyntax extensionsClass)
        {
            List<ParameterSyntax> parameters = new List<ParameterSyntax>()
            {
                SF.Parameter(SF.Identifier("logger"))
                    .AddModifiers(SF.Token(SyntaxKind.ThisKeyword))
                    .WithType(SF.ParseTypeName(Constants.ILoggerTypeName))
            };
            parameters.AddRange(messageMethod.Parameters.Select(p =>
                SF.Parameter(SF.Identifier(p)).WithType(SF.ParseTypeName("object"))));

            List<ArgumentSyntax> arguments = new List<ArgumentSyntax>()
            {
                SF.Argument(SF.IdentifierName("logger"))
            };
            arguments.AddRange(messageMethod.Parameters.Select(p => SF.Argument(SF.IdentifierName(p))));

            var methodName = messageMethod.GetMethodName(messageMethod.Id);

            var summary = $"/// <summary>\n\t/// {messageMethod.MessageTemplate}\n\t/// </summary>\n\t";

            // Create method
            return SF.MethodDeclaration(SF.ParseName("void"), methodName)
                .AddModifiers(
                    SF.Token(SyntaxKind.PublicKeyword),
                    SF.Token(SyntaxKind.StaticKeyword))
                .AddParameterListParameters(parameters.ToArray())
                .WithBody(
                    SF.Block(
                        SF.ExpressionStatement(SF.InvocationExpression(SF.IdentifierName($"_{methodName}"), 
                            SF.ArgumentList(SF.SeparatedList(arguments))))))
                .WithLeadingTrivia(SF.ParseLeadingTrivia(summary))                
                .NormalizeWhitespace();
        }

        private FieldDeclarationSyntax CreateFieldDeclaration(MessageMethod messageMethod, ClassDeclarationSyntax extensionsClass)
        {
            var genericArguments = new List<TypeSyntax> {SF.IdentifierName(Constants.ILoggerTypeName)};
            genericArguments.AddRange(messageMethod.Parameters.Select(p => SF.PredefinedType(SF.Token(SyntaxKind.ObjectKeyword))));

            var variableName = $"_{messageMethod.GetMethodName(messageMethod.Id)}";

            return SF.FieldDeclaration(SF.VariableDeclaration(SF.GenericName(SF.Identifier("Action")).AddTypeArgumentListArguments(genericArguments.ToArray())))
                .AddModifiers(
                    SF.Token(SyntaxKind.PrivateKeyword),
                    SF.Token(SyntaxKind.StaticKeyword),
                    SF.Token(SyntaxKind.ReadOnlyKeyword))
                .AddDeclarationVariables(SF.VariableDeclarator(variableName)).NormalizeWhitespace();
        }


        private ExpressionStatementSyntax CreateExpressionStatement(MessageMethod messageMethod, ClassDeclarationSyntax extensionsClass)
        {
            var genericArguments = new List<TypeSyntax>();
            genericArguments.AddRange(messageMethod.Parameters.Select(p => SF.PredefinedType(SF.Token(SyntaxKind.ObjectKeyword))));

            var fieldName = $"_{messageMethod.GetMethodName(messageMethod.Id)}";

            return SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                SF.IdentifierName(fieldName),
                SF.InvocationExpression(
                        SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("LoggerMessage"),
                            SF.GenericName(SF.Identifier("Define"))
                                .AddTypeArgumentListArguments(genericArguments.ToArray())))
                    .AddArgumentListArguments(
                        SF.Argument(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("LogLevel"), SF.IdentifierName("Error"))),
                        SF.Argument(SF.ObjectCreationExpression(SF.IdentifierName("EventId"))
                            .AddArgumentListArguments(
                                SF.Argument(SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.ParseToken("17"))),
                                SF.Argument(SF.InvocationExpression(SF.IdentifierName("nameof"))
                                    .AddArgumentListArguments(
                                        SF.Argument(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("LoggerMessages"), SF.IdentifierName(messageMethod.Id))))),
                                SF.Argument(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("LoggerMessages"), SF.IdentifierName(messageMethod.Id)))))
                    ))).NormalizeWhitespace();
        }


        public MessageMethod AddOrUpdateMethod(MessageMethod messageMethod, ClassDeclarationSyntax classDeclaration,
            ViewParams viewParams)
        {
            var existsMethod = MessageMethods.FirstOrDefault(m => m.Id == $"{viewParams.OldAbbr}{messageMethod.Number:D5}");
            if (existsMethod != null)
            {
                if (!viewParams.NewTemplate.Equals(existsMethod.MessageTemplate) ||
                    !viewParams.NewAbbr.Equals(existsMethod.Abbr) ||
                    viewParams.NewLevel != existsMethod.Level)
                {
                    existsMethod.MessageTemplate = viewParams.NewTemplate;
                    existsMethod.Abbr = viewParams.NewAbbr;
                    existsMethod.Level = viewParams.NewLevel;
                    existsMethod.MethodDeclaration = CreateMethodDeclaration(messageMethod, classDeclaration);
                    existsMethod.FieldDeclaration = CreateFieldDeclaration(messageMethod, classDeclaration);
                    existsMethod.ExpressionStatement = CreateExpressionStatement(messageMethod, classDeclaration);
                }
                return existsMethod;
            }

            var lastMethodNumber = MessageMethods.Where(m => m.Abbr == viewParams.NewAbbr).OrderBy(m => m.Number).LastOrDefault();
            messageMethod.Number = lastMethodNumber == null ? 1 : lastMethodNumber.Number + 1;
            messageMethod.MessageTemplate = viewParams.NewTemplate;
            messageMethod.Abbr = viewParams.NewAbbr;
            messageMethod.Level = viewParams.NewLevel;
            messageMethod.MethodDeclaration = CreateMethodDeclaration(messageMethod, classDeclaration);
            messageMethod.FieldDeclaration = CreateFieldDeclaration(messageMethod, classDeclaration);
            messageMethod.ExpressionStatement = CreateExpressionStatement(messageMethod, classDeclaration);
            MessageMethods.Add(messageMethod);
            return messageMethod;
        }
    }
}
