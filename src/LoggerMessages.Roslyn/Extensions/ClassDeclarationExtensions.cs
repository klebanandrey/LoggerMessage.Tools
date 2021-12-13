using System.Linq;
using LoggerMessages.Common;
using LoggerMessages.Roslyn.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Constants = EventGroups.Roslyn.Constants;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LoggerMessages.Roslyn
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

        private static CompilationUnitSyntax GetCompilationUnitSyntax(SyntaxNode node)
        {
            while (!(node is CompilationUnitSyntax))
                node = node.Parent;

            return node as CompilationUnitSyntax;
        }


        public static string GetOrCreateLoggerVariable(this ClassDeclarationSyntax classDeclaration, SemanticModel model, ref Document document)
        {
            if (TryGetExistsLoggerMember(model, classDeclaration, out var loggerVariable))
                return loggerVariable;

            var loggerField = SF.FieldDeclaration(SF.VariableDeclaration(SF.ParseTypeName(Constants.ILoggerTypeName),
                    SF.SeparatedList(new[] { SF.VariableDeclarator(SF.Identifier("_logger")) })
                ))
                .AddModifiers(SF.Token(SyntaxKind.PrivateKeyword), SF.Token(SyntaxKind.ReadOnlyKeyword));

            var newClassDeclaration = classDeclaration.AddMembers(loggerField.NormalizeWhitespace());

            var root = GetCompilationUnitSyntax(classDeclaration);

            root = root.ReplaceNode(classDeclaration, newClassDeclaration);

            if (root.Usings.All(u => u.Name != SF.ParseName(Constants.ILoggerModuleNamespace)))
                root = root.AddUsings(SF.UsingDirective(SF.ParseName(Constants.ILoggerModuleNamespace))).NormalizeWhitespace();

            document = document.WithSyntaxRoot(root);

            return loggerField.Declaration.Variables.OfType<VariableDeclaratorSyntax>().FirstOrDefault().Identifier.Text;
        }

        public static ClassDeclarationSyntax AddCall(this ClassDeclarationSyntax classDeclaration, LoggerMessage message, int rowNumber, int columnNumber, ref Document document)
        {
            var blockSyntax = classDeclaration.DescendantNodes().OfType<BlockSyntax>().LastOrDefault(c =>
                c.SyntaxTree.GetLineSpan(c.Span).StartLinePosition.Line <= rowNumber &&
                c.SyntaxTree.GetLineSpan(c.Span).EndLinePosition.Line >= rowNumber);

            var newBlockSyntax = blockSyntax.AddStatements(message.GetMethodCallExpression());

            var root = GetCompilationUnitSyntax(classDeclaration);

            root = root.ReplaceNode(blockSyntax, newBlockSyntax);

            document = document.WithSyntaxRoot(root);

            return classDeclaration;
        }
    }
}
