using System.Linq;
using EventGroups.Roslyn;
using LoggerMessages.Roslyn.Exceptions;
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


        private static bool InsideInClass(ClassDeclarationSyntax classDeclaration, int rowNumber, int columnNumber)
        {
            var lineSpan = classDeclaration.SyntaxTree.GetLineSpan(classDeclaration.Span);
            rowNumber--;

            if (lineSpan.StartLinePosition.Line < rowNumber && lineSpan.EndLinePosition.Line > rowNumber)
                return true;

            if (lineSpan.StartLinePosition.Line == rowNumber &&
                lineSpan.StartLinePosition.Character >= columnNumber &&
                lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line)
                return true;

            if (lineSpan.EndLinePosition.Line == rowNumber &&
                lineSpan.EndLinePosition.Character <= columnNumber &&
                lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line)
                return true;

            return lineSpan.StartLinePosition.Line == lineSpan.EndLinePosition.Line &&
                   lineSpan.StartLinePosition.Character <= columnNumber &&
                   lineSpan.EndLinePosition.Character >= columnNumber;
        }

        public static ClassDeclarationSyntax GetClassDeclaration(this Document document, int rowNumber, int columnNumber, out SemanticModel model)
        {
            var root = document.GetSyntaxRootAsync().Result as CompilationUnitSyntax;

            var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault(c => InsideInClass(c, rowNumber, columnNumber));

            if (classDeclaration == null)
                throw new WrongPositionException();

            var compilation = CSharpCompilation.Create(document.Project.AssemblyName)
                .AddReferences(document.Project.MetadataReferences)
                .AddSyntaxTrees(classDeclaration.SyntaxTree);

            model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            return classDeclaration;
        }
    }
}
