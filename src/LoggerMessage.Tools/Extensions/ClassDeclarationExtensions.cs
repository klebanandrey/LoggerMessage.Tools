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

        public static string GetOrCreateLoggerField(this ClassDeclarationSyntax classDeclaration, SemanticModel model, out FieldDeclarationSyntax loggerFieldDeclaration)
        {
            loggerFieldDeclaration = null;
            var loggerProperty = classDeclaration.DescendantNodes<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => IsLoggerSymbol(p.Type, model));

            if (loggerProperty != null)
                return loggerProperty.Identifier.Text;

            var loggerField = classDeclaration.DescendantNodes().OfType<FieldDeclarationSyntax>()
                .FirstOrDefault(f => IsLoggerSymbol(f.Declaration.Type, model));

            if (loggerField != null)
            {
                var existVariable = loggerField.Declaration.Variables.OfType<VariableDeclaratorSyntax>().FirstOrDefault();
                if (existVariable != null)
                    return existVariable.Identifier.Text;
            }

            loggerFieldDeclaration = SF.FieldDeclaration(SF.VariableDeclaration(SF.ParseTypeName(Constants.ILoggerTypeName),
                    SF.SeparatedList(new[] { SF.VariableDeclarator(SF.Identifier(Constants.LoggerVariable)) })))
                .AddModifiers(SF.Token(SyntaxKind.PrivateKeyword), SF.Token(SyntaxKind.ReadOnlyKeyword));
            return Constants.LoggerVariable;
        }
    }
}
