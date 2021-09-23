using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EventGroups.Roslyn
{
    public static class DocumentExtensions
    {
        public static SyntaxNode CreateLoggerMessageClass(this Document document)
        {
            var root = document.GetSyntaxRootAsync().Result as CompilationUnitSyntax;
            root = root.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{document.Project.Name}.Properties")));

            var nsStr = string.IsNullOrWhiteSpace(document.Project.DefaultNamespace)
                ? Constants.DefaultNamespace
                : document.Project.DefaultNamespace;

            var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(nsStr));

            //  Create a class: (class Order)
            var classDeclaration = SyntaxFactory.ClassDeclaration(Constants.ClassName);

            // Add the public modifier: (public static class Order)
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            ns = ns.AddMembers(classDeclaration);

            return root.AddMembers(ns).NormalizeWhitespace();
        }

    }
}
