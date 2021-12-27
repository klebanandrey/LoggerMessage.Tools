using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoggerMessage.Shared;
using LoggerMessage.Tools.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LoggerMessage.Tools
{
    public class LoggerExtensions
    {
        private LoggerExtensions()
        {
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
            if (expressions == null)
                return null;

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
            var extensions = new LoggerExtensions();
            var classNodes = extensionsDocument.GetClassDeclaration().DescendantNodes();

            var methods = classNodes.OfType<MethodDeclarationSyntax>();
            var fields = classNodes.OfType<FieldDeclarationSyntax>();
            var constructorDeclaration = classNodes.OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
            var expressions = constructorDeclaration == null ? null : constructorDeclaration.DescendantNodes<ExpressionStatementSyntax>();

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

        private MethodDeclarationSyntax CreateMethodDeclaration(MessageMethod messageMethod)
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
            arguments.Add(SF.Argument(SF.IdentifierName("null")));

            var methodName = messageMethod.GetMethodName();

            var summary = $"{Environment.NewLine}/// <summary>{Environment.NewLine}/// {messageMethod.MessageTemplate}{Environment.NewLine}/// </summary>{Environment.NewLine}";

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

        private FieldDeclarationSyntax CreateFieldDeclaration(MessageMethod messageMethod)
        {
            var genericArguments = new List<TypeSyntax> {SF.IdentifierName(Constants.ILoggerTypeName)};
            genericArguments.AddRange(messageMethod.Parameters.Select(p => SF.PredefinedType(SF.Token(SyntaxKind.ObjectKeyword))));
            genericArguments.Add(SF.IdentifierName("Exception"));

            var variableName = $"_{messageMethod.GetMethodName()}";

            return SF.FieldDeclaration(SF.VariableDeclaration(SF.GenericName(SF.Identifier("Action")).AddTypeArgumentListArguments(genericArguments.ToArray())))
                .AddModifiers(
                    SF.Token(SyntaxKind.PrivateKeyword),
                    SF.Token(SyntaxKind.StaticKeyword),
                    SF.Token(SyntaxKind.ReadOnlyKeyword))
                .AddDeclarationVariables(SF.VariableDeclarator(variableName)).NormalizeWhitespace();
        }


        private ExpressionStatementSyntax CreateExpressionStatement(MessageMethod messageMethod)
        {
            var genericArguments = new List<TypeSyntax>();
            genericArguments.AddRange(
                messageMethod.Parameters.Select(p => SF.PredefinedType(SF.Token(SyntaxKind.ObjectKeyword))));

            var fieldName = $"_{messageMethod.GetMethodName()}";

            var memberAccess = messageMethod.Parameters.Count > 0
                ? SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("LoggerMessage"),
                    SF.GenericName(SF.Identifier("Define")).AddTypeArgumentListArguments(genericArguments.ToArray()))
                : SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("LoggerMessage"),
                    SF.IdentifierName("Define"));

            return SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                SF.IdentifierName(fieldName),
                SF.InvocationExpression(memberAccess)
                    .AddArgumentListArguments(
                        SF.Argument(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SF.IdentifierName("LogLevel"), SF.IdentifierName("Error"))),
                        SF.Argument(SF.ObjectCreationExpression(SF.IdentifierName("EventId"))
                            .AddArgumentListArguments(
                                SF.Argument(SF.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                    SF.ParseToken("17"))),
                                SF.Argument(SF.InvocationExpression(SF.IdentifierName("nameof"))
                                    .AddArgumentListArguments(
                                        SF.Argument(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            SF.IdentifierName("LoggerMessages"), SF.IdentifierName(messageMethod.Id)))))
                            )),
                        SF.Argument(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SF.IdentifierName("LoggerMessages"), SF.IdentifierName(messageMethod.Id)))
                    ))).NormalizeWhitespace();
        }


        public MessageMethod AddOrUpdateMessageMethod(MessageMethod messageMethod, ClassDeclarationSyntax classDeclaration,
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
                    existsMethod.MethodDeclaration = CreateMethodDeclaration(messageMethod);
                    existsMethod.FieldDeclaration = CreateFieldDeclaration(messageMethod);
                    existsMethod.ExpressionStatement = CreateExpressionStatement(messageMethod);
                }
                return existsMethod;
            }

            var lastMethodNumber = MessageMethods.Where(m => m.Abbr == viewParams.NewAbbr).OrderBy(m => m.Number).LastOrDefault();
            messageMethod.Number = lastMethodNumber == null ? 1 : lastMethodNumber.Number + 1;
            messageMethod.MessageTemplate = viewParams.NewTemplate;
            messageMethod.Abbr = viewParams.NewAbbr;
            messageMethod.Level = viewParams.NewLevel;
            messageMethod.MethodDeclaration = CreateMethodDeclaration(messageMethod);
            messageMethod.FieldDeclaration = CreateFieldDeclaration(messageMethod);
            messageMethod.ExpressionStatement = CreateExpressionStatement(messageMethod);
            MessageMethods.Add(messageMethod);
            return messageMethod;
        }

        public async Task<Document> FillExtensionsFile(Document extensionsFile)
        {
            var fields = MessageMethods.Select(m => m.FieldDeclaration);
            var methods = MessageMethods.Select(m => m.MethodDeclaration);
            var expressions = MessageMethods.Select(m => m.ExpressionStatement);

            var classDeclaration = extensionsFile.GetClassDeclaration();
            var root = await extensionsFile.GetSyntaxRootAsync();

            var newClass = extensionsFile.CreateEmptyClassDeclaration();
            newClass = newClass.AddMembers(fields.ToArray()).AddMembers(methods.ToArray());
            var constructor = newClass.DescendantNodes<ConstructorDeclarationSyntax>().FirstOrDefault();
            var newConstructor = constructor.AddBodyStatements(expressions.ToArray());

            newClass = newClass.ReplaceNode(constructor, newConstructor);
            root = root.ReplaceNode(classDeclaration, newClass).NormalizeWhitespace();
            return extensionsFile.WithSyntaxRoot(root);
        }
    }
}
