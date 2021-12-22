using System.Linq;
using EventGroups.Roslyn;
using LoggerMessages.Roslyn.Exceptions;
using LoggerMessages.Roslyn.Extensions;
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



        public static ClassDeclarationSyntax GetClassDeclaration(this Document document, int rowNumber = 0, int columnNumber = 0)
        {
            var root = document.GetSyntaxRootAsync().Result as CompilationUnitSyntax;

            var classDeclaration = (rowNumber == 0 && columnNumber == 0) 
                ? root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault()
                : root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault(c => c.InsideNode(rowNumber, columnNumber));

            if (classDeclaration == null)
                throw new WrongPositionException();

            return classDeclaration;
        }
    }
}
