#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LoggerMessage.Tools.Extensions
{
    public static class SyntaxNodeExtensions
    {
        public static bool InsideNode(this SyntaxNode node, int rowNumber, int columnNumber)
        {
            var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
            rowNumber--;

            if ((lineSpan.StartLinePosition.Line <= rowNumber && lineSpan.EndLinePosition.Line > rowNumber) ||
                (lineSpan.StartLinePosition.Line < rowNumber && lineSpan.EndLinePosition.Line >= rowNumber))
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
                   lineSpan.StartLinePosition.Line == rowNumber &&
                   lineSpan.StartLinePosition.Character <= columnNumber &&
                   lineSpan.EndLinePosition.Character >= columnNumber;
        }

        public static IEnumerable<TNode> DescendantNodes<TNode>(this SyntaxNode node, Func<SyntaxNode, bool>? descendIntoChildren = null, bool descendIntoTrivia = false) where TNode : SyntaxNode
        {
            return node.DescendantNodes(descendIntoChildren, descendIntoTrivia).OfType<TNode>();
        }

        public static NamespaceDeclarationSyntax? GetNamespaceDeclaration(this SyntaxNode node)
        {
            while (!(node is NamespaceDeclarationSyntax))
            {
                if (node.Parent == null)
                    return null;
                node = node.Parent;
            }

            return node as NamespaceDeclarationSyntax;
        }
    }
}
