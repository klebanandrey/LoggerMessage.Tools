using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LoggerMessage.Tools.Extensions
{
    public static class ClassDeclarationExtensions
    {
        private static bool IsLoggerSymbol(TypeSyntax type, SemanticModel model)
        {
            var ti = model.GetTypeInfo(type).Type;

            //var si = model.GetSymbolInfo(type).Symbol;

            if (ti == null)
                return false;
            return ti.Name == Constants.ILoggerTypeName;
        }


        private static bool TryGetExistsLoggerMember(SemanticModel model, ClassDeclarationSyntax classDeclaration, out string loggerMemberName)
        {
            var loggerProperty = classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => IsLoggerSymbol(p.Type, model));

            if (loggerProperty != null)
            {
                loggerMemberName = loggerProperty.Identifier.Text;
                return true;
            }

            var loggerField = classDeclaration.DescendantNodes().OfType<FieldDeclarationSyntax>()
                .FirstOrDefault(f => IsLoggerSymbol(f.Declaration.Type, model));

            if (loggerField != null)
            {
                loggerMemberName = loggerField.Declaration.Variables.OfType<VariableDeclaratorSyntax>().FirstOrDefault().Identifier
                    .Text;
                return true;
            }

            loggerMemberName = string.Empty;
            return false;
        }


        public static string GetOrCreateLoggerVariableName(this ClassDeclarationSyntax classDeclaration, SemanticModel model, ref Document document, out FieldDeclarationSyntax loggerFieldDeclaration)
        {
            loggerFieldDeclaration = null;
            if (TryGetExistsLoggerMember(model, classDeclaration, out var loggerVariable))
                return loggerVariable;

            loggerFieldDeclaration = SF.FieldDeclaration(SF.VariableDeclaration(SF.ParseTypeName(Constants.ILoggerTypeName),
                    SF.SeparatedList(new[] { SF.VariableDeclarator(SF.Identifier("_logger")) })))
                .AddModifiers(SF.Token(SyntaxKind.PrivateKeyword), SF.Token(SyntaxKind.ReadOnlyKeyword));
            return Constants.LoggerVariable;
        }

        public static ClassDeclarationSyntax AddCall(this ClassDeclarationSyntax classDeclaration, ExpressionStatementSyntax expression, FieldDeclarationSyntax loggerFieldDecalration, int rowNumber, int columnNumber, ref Document document)
        {
            var root = classDeclaration.SyntaxTree.GetCompilationUnitRoot();
            var blockSyntax = classDeclaration.DescendantNodes().OfType<BlockSyntax>().LastOrDefault(c =>
                c.SyntaxTree.GetLineSpan(c.Span).StartLinePosition.Line <= rowNumber &&
                c.SyntaxTree.GetLineSpan(c.Span).EndLinePosition.Line >= rowNumber);

            var newBlockSyntax = blockSyntax.AddStatements(expression);
            
            var newClassDeclaration = classDeclaration.ReplaceNode(blockSyntax, newBlockSyntax);

            if (loggerFieldDecalration != null)
                newClassDeclaration = newClassDeclaration.AddMembers(loggerFieldDecalration.NormalizeWhitespace());

            var loggerMessagesNamespace = document.Project.GetNamespace();

            root = root.ReplaceNode(classDeclaration, newClassDeclaration);

            var usingDirectives = new List<UsingDirectiveSyntax>();

            if (root.Usings.All(u => u.Name.ToString() != SF.ParseName(Constants.ILoggerModuleNamespace).ToString()))
                usingDirectives.Add(SF.UsingDirective(SF.ParseName(Constants.ILoggerModuleNamespace)).NormalizeWhitespace()
                    .WithTrailingTrivia(SF.EndOfLine(Environment.NewLine)));

            if (classDeclaration.GetNamespaceDeclaration().Name.ToString() != loggerMessagesNamespace &&
                root.Usings.All(u => u.Name.ToString() != SF.ParseName(loggerMessagesNamespace).ToString()))
                usingDirectives.Add(SF.UsingDirective(SF.ParseName(loggerMessagesNamespace)).NormalizeWhitespace()
                    .WithTrailingTrivia(SF.EndOfLine(Environment.NewLine))); 

            root = root.AddUsings(usingDirectives.ToArray());

            document = document.WithSyntaxRoot(root);

            return newClassDeclaration;
        }
    }
}
