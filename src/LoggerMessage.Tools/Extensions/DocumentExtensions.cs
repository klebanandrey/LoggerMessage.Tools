﻿using System.Linq;
using LoggerMessage.Tools.Exceptions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LoggerMessage.Tools.Extensions
{
    public static class DocumentExtensions
    {
        public static SyntaxNode CreateExtensionsDocument(this Document document)
        {
            var root = document.GetSyntaxRootAsync().Result as CompilationUnitSyntax;
            root = root.AddUsings(SF.UsingDirective(SF.ParseName($"{document.Project.Name}.Properties")));

            var ns = SF.NamespaceDeclaration(SF.ParseName(document.Project.GetNamespace()));

            ns = ns.AddMembers(document.CreateEmptyClassDeclaration());
            return root.AddMembers(ns).NormalizeWhitespace();
        }

        public static ClassDeclarationSyntax CreateEmptyClassDeclaration(this Document document)
        {
            var classDeclaration = SF.ClassDeclaration(Constants.ClassName);
            classDeclaration = classDeclaration.AddModifiers(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.StaticKeyword));
            classDeclaration = classDeclaration.AddMembers(SF.ConstructorDeclaration(Constants.ClassName).AddModifiers(SF.Token(SyntaxKind.StaticKeyword)));
            return classDeclaration.NormalizeWhitespace();
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
