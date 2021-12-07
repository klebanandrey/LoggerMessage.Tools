using System.Linq;
using EventGroups.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LoggerMessages.Roslyn
{
    public static class DocumentExtensions
    {
        public static SyntaxNode CreateLoggerMessageClass(this Document document)
        {
            var root = document.GetSyntaxRootAsync().Result as CompilationUnitSyntax;
            root = root.AddUsings(SF.UsingDirective(SF.ParseName($"{document.Project.Name}.Properties")));

            var nsStr = string.IsNullOrWhiteSpace(document.Project.DefaultNamespace)
                ? Constants.DefaultNamespace
                : document.Project.DefaultNamespace;

            var ns = SF.NamespaceDeclaration(SF.ParseName(nsStr));

            //  Create a class: (class Order)
            var classDeclaration = SF.ClassDeclaration(Constants.ClassName);

            // Add the public modifier: (public static class Order)
            classDeclaration = classDeclaration.AddModifiers(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.StaticKeyword));

            ns = ns.AddMembers(classDeclaration);

            return root.AddMembers(ns).NormalizeWhitespace();
        }

        private static bool IsLoggerSymbol(TypeSyntax type, SemanticModel model)
        {
            var si = model.GetSymbolInfo(type).Symbol;
            if (si == null)
                return false;
            return si.Name == Constants.ILoggerTypeName &&
                   si.ContainingModule.Name == Constants.ILoggerModuleName;
        }

        private static bool TryGetExistsLoggerMember(Document document, ClassDeclarationSyntax classDeclaration, out string loggerMemberName)
        {
            var compilation = CSharpCompilation.Create(document.Project.AssemblyName)
                .AddReferences(document.Project.MetadataReferences)
                .AddSyntaxTrees(classDeclaration.SyntaxTree);
            
            SemanticModel model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);

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


        public static Document GetOrCreateLoggerVariable(this Document document, int rowNumber, out string loggerVariable)
        {
            var root = document.GetSyntaxRootAsync().Result as CompilationUnitSyntax;

            var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault(c =>
                c.SyntaxTree.GetLineSpan(c.Span).StartLinePosition.Line <= rowNumber &&
                c.SyntaxTree.GetLineSpan(c.Span).EndLinePosition.Line >= rowNumber);

            if (TryGetExistsLoggerMember(document, classDeclaration, out loggerVariable))
                return document;

            var loggerField = SF.FieldDeclaration(SF.VariableDeclaration(SF.ParseTypeName(Constants.ILoggerTypeName),
                        SF.SeparatedList(new[] { SF.VariableDeclarator(SF.Identifier("_logger")) })
                    ))
                .AddModifiers(SF.Token(SyntaxKind.PrivateKeyword), SF.Token(SyntaxKind.ReadOnlyKeyword));

            var newClassDeclaration = classDeclaration.AddMembers(loggerField.NormalizeWhitespace());

            root = root.ReplaceNode(classDeclaration, newClassDeclaration);
            
            if (root.Usings.All(u => u.Name != SF.ParseName(Constants.ILoggerModuleNamespace)))
                root = root.AddUsings(SF.UsingDirective(SF.ParseName(Constants.ILoggerModuleNamespace))).NormalizeWhitespace();

            document = document.WithSyntaxRoot(root);

            loggerVariable = loggerField.Declaration.Variables.OfType<VariableDeclaratorSyntax>().FirstOrDefault().Identifier.Text;
            return document;
        }

        public static Document AddCall(this Document document, LoggerMessage message, int rowNumber, string loggerVariable)
        {
            var root = document.GetSyntaxRootAsync().Result as CompilationUnitSyntax;
            var currentNode = root.DescendantNodes().LastOrDefault(c =>
                c.SyntaxTree.GetLineSpan(c.Span).StartLinePosition.Line <= rowNumber &&
                c.SyntaxTree.GetLineSpan(c.Span).EndLinePosition.Line >= rowNumber);

            var expression = message.GetMethodCallExpression(loggerVariable);

            return document;
        }
    }
}
