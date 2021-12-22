using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LoggerMessage.Shared;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LoggerMessages.Roslyn.Extensions
{
    public static class MethodDeclarationSyntaxExtensions
    {
        public static string GetIdFromMethodDeclaration(this MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.Identifier.Text.Split('_')[0];
        }

        public static Level GetLevelFromMethodDeclaration(this MethodDeclarationSyntax methodDeclaration)
        {
            return (Level)Enum.Parse(typeof(Level), methodDeclaration.Identifier.Text.Split('_')[1]);
        }

        public static string GetTemplateFromMethodDeclaration(this MethodDeclarationSyntax methodDeclaration)
        {
            var trivia = methodDeclaration.GetLeadingTrivia().Single(t => t.Kind() == SyntaxKind.SingleLineDocumentationCommentTrivia);
            var xml = trivia.GetStructure();
            var summaryText = xml.DescendantNodes().OfType<XmlElementSyntax>().FirstOrDefault().Content.ToString();
            summaryText = summaryText.Replace("///", "");
            summaryText = Regex.Replace(summaryText, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline).TrimEnd();
            return summaryText.TrimStart(' ');
        }

    }
}
