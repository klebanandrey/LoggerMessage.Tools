using System;
using System.Linq;
using System.Threading.Tasks;
using LoggerMessage.Shared;
using LoggerMessage.Shared.Services;
using LoggerMessage.Tools.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LoggerMessage.Tools
{
    public class MessageService
    {
        private readonly IEventGroupService _eventGroupService;

        public MessageService(IEventGroupService eventGroupService)
        {
            _eventGroupService = eventGroupService;
        }

        public Document GetOrCreateLoggerMessagesExtensions(Project project)
        {
            var existFile = project.Documents.FirstOrDefault(d =>
                d.Name.Equals(Constants.LoggerMessagesExtensionsFileName, StringComparison.OrdinalIgnoreCase));
            if (existFile != null)
                return existFile;

            var newDoc = project.AddDocument(Constants.LoggerMessagesExtensionsFileName, Constants.DefaultContent,
                new[] { Constants.LoggerMessagesFolderName });
            return newDoc.WithSyntaxRoot(newDoc.CreateLoggerExtensions());
        }

        public TextDocument GetOrCreateLoggerMessagesResx(Project project)
        {
            var existFile = project.AdditionalDocuments.FirstOrDefault(d =>
                d.Name.Equals(Shared.Constants.LoggerMessagesResxFileName, StringComparison.OrdinalIgnoreCase));
            if (existFile != null)
                return existFile;
            return project.CreateLoggerMessagesResx();
        }

        private ExpressionStatementSyntax GetCurrentNode(ClassDeclarationSyntax classDeclaration, int rowNumber, int columnNumber)
        {
            return classDeclaration.DescendantNodes<ExpressionStatementSyntax>()
                .FirstOrDefault(n => n.InsideNode(rowNumber, columnNumber));
        }

        private string GetMethodIdFromCurrentInvocation(ExpressionStatementSyntax expressionNode)
        {
            var memberAccess = expressionNode.DescendantNodes<MemberAccessExpressionSyntax>().FirstOrDefault();
            var methodName = memberAccess.Name.ToString();
            return methodName.Split('_')[0];
        }

        private MethodDeclarationSyntax GetMethodDeclarationByName(SyntaxNode nodeRoot, string methodName)
        {
            return nodeRoot.DescendantNodes<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.Text == methodName);
        }


        public async Task<MessageMethod> GetLoggerMessage(LoggerExtensions extensions,
            ClassDeclarationSyntax classDeclaration, int rowNumber, int columnNumber)
        {
            var currentNode = GetCurrentNode(classDeclaration, rowNumber, columnNumber);
            MessageMethod result;

            if (currentNode == null || string.IsNullOrEmpty(GetMethodIdFromCurrentInvocation(currentNode)))
                result = MessageMethod.Create("", "");
            else
            {
                var methodId = GetMethodIdFromCurrentInvocation(currentNode);
                result = extensions.MessageMethods.FirstOrDefault(m => m.Id == methodId);
            }

            return result;
        }

        public ExpressionStatementSyntax GetLoggerMessageMethodInvocation(Document document, ClassDeclarationSyntax classDeclaration,
            MessageMethod messageMethod, LoggerExtensions extensions, ViewParams viewParams)
        {
            var compilation = CSharpCompilation.Create(document.Project.AssemblyName)
                .AddReferences(document.Project.MetadataReferences)
                .AddSyntaxTrees(classDeclaration.SyntaxTree);

            var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);

            messageMethod = extensions.AddOrUpdateMethod(messageMethod, classDeclaration, viewParams);

            var loggerVariable = classDeclaration.GetOrCreateLoggerVariable(model, ref document);

            return messageMethod.GetMethodCallExpression(loggerVariable, messageMethod.Id).NormalizeWhitespace();
        }
    }
}
