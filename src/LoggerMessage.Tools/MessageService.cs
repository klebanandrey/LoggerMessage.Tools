using System;
using System.IO;
using System.Linq;
using System.Resources.NetStandard;
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

        private LoggerExtensions _loggerExtensions;

        private DocumentId _currentDocumentId;

        private ClassDeclarationSyntax _currentClassDeclaration;
        private DocumentId _extensionsDocumentId;
        private DocumentId _resxDocumentId;
        public Project CurrentProject { get; private set; }

        public MessageService(IEventGroupService eventGroupService)
        {
            _eventGroupService = eventGroupService;
        }

        public void Initialize(Project currentProject, DocumentId currentDocumentId, int rowNumber = 0, int columnNumber = 0)
        {
            CurrentProject = currentProject;
            _currentDocumentId = currentDocumentId;
            _currentClassDeclaration = CurrentProject.GetDocument(_currentDocumentId).GetClassDeclaration(rowNumber, columnNumber);

            CurrentProject = GetOrCreateMessagesResx(CurrentProject, out var resxDocument);
            _resxDocumentId = resxDocument.Id;

            CurrentProject = GetOrCreateExtensionsDocument(CurrentProject, out var extensionsDocument);
            _extensionsDocumentId = extensionsDocument.Id;

            _loggerExtensions = LoggerExtensions.Init(extensionsDocument);
        }

        public async Task WriteExtensionsToFiles()
        {
            var extensionsDocument = CurrentProject.GetDocument(_extensionsDocumentId);
            extensionsDocument = await _loggerExtensions.FillExtensionsFile(extensionsDocument);
            CurrentProject = extensionsDocument.Project;
        }

        public async Task AddMessageToResource(MessageMethod message)
        {
            var resxDocument = CurrentProject.GetAdditionalDocument(_resxDocumentId);
            CurrentProject = CurrentProject.AddOrUpdateResource(message, ref resxDocument);

            CurrentProject = CurrentProject.GenerateResxClass();
        }

        public Project GetOrCreateExtensionsDocument(Project project, out Document extensionsDocument)
        {
            extensionsDocument = project.Documents.FirstOrDefault(d =>
                d.Name.Equals(Constants.LoggerMessagesExtensionsFileName, StringComparison.OrdinalIgnoreCase));
            if (extensionsDocument == null)
            {
                extensionsDocument = project.AddDocument(Constants.LoggerMessagesExtensionsFileName, Constants.DefaultContent,
                    new[] { Constants.LoggerMessagesFolderName });
                
                extensionsDocument = extensionsDocument.WithSyntaxRoot(extensionsDocument.CreateExtensionsDocument());
            }

            return extensionsDocument.Project;
        }

        public static TextDocument CreateMessagesResx(Project project)
        {
            var tmpFilePath = Path.Combine(Path.GetTempPath(), Shared.Constants.LoggerMessagesResxFileName);
            using (ResXResourceWriter resx = new ResXResourceWriter(tmpFilePath))
            {
                resx.Generate();
                resx.Close();
            }

            var realFilePath = Path.Combine(Path.GetDirectoryName(project.FilePath),
                Shared.Constants.LoggerMessagesResxFolderName, Shared.Constants.LoggerMessagesResxFileName);

            return project.AddAdditionalDocument(Shared.Constants.LoggerMessagesResxFileName,
                File.ReadAllText(tmpFilePath), new[] { Shared.Constants.LoggerMessagesResxFolderName }, realFilePath);
        }

        public Project GetOrCreateMessagesResx(Project project, out TextDocument resxDocument)
        {
            resxDocument = project.AdditionalDocuments.FirstOrDefault(d =>
                d.Name.Equals(Shared.Constants.LoggerMessagesResxFileName, StringComparison.OrdinalIgnoreCase));
            if (resxDocument == null)
                resxDocument = CreateMessagesResx(project);

            return resxDocument.Project;
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


        public async Task<MessageMethod> GetLoggerMessage(int rowNumber, int columnNumber)
        {
            var currentNode = GetCurrentNode(_currentClassDeclaration, rowNumber, columnNumber);
            MessageMethod result;

            if (currentNode == null || string.IsNullOrEmpty(GetMethodIdFromCurrentInvocation(currentNode)))
                result = MessageMethod.Create("", "");
            else
            {
                var methodId = GetMethodIdFromCurrentInvocation(currentNode);
                result = _loggerExtensions.MessageMethods.FirstOrDefault(m => m.Id == methodId);
            }

            return result;
        }

        public ExpressionStatementSyntax GetLoggerMessageMethodInvocation(MessageMethod messageMethod, ViewParams viewParams, out FieldDeclarationSyntax loggerFieldDeclaration)
        {
            var compilation = CSharpCompilation.Create(CurrentProject.AssemblyName)
                .AddReferences(CurrentProject.MetadataReferences)
                .AddSyntaxTrees(_currentClassDeclaration.SyntaxTree);

            var model = compilation.GetSemanticModel(_currentClassDeclaration.SyntaxTree);

            messageMethod = _loggerExtensions.AddOrUpdateMethod(messageMethod, _currentClassDeclaration, viewParams);

            var currentDocument = CurrentProject.GetDocument(_currentDocumentId);
            var loggerVariable = _currentClassDeclaration.GetOrCreateLoggerVariableName(model, ref currentDocument, out loggerFieldDeclaration);
            CurrentProject = currentDocument.Project;

            return messageMethod.GetMethodCallExpression(loggerVariable, messageMethod.Id).NormalizeWhitespace();
        }

        public void AddInvocation(ExpressionStatementSyntax expression, FieldDeclarationSyntax loggerFieldDeclaration, int rowNumber, int columnNumber)
        {
            var currentDocument = CurrentProject.GetDocument(_currentDocumentId);
            var currentClassDeclaration = currentDocument.GetClassDeclaration(rowNumber, columnNumber);
            _currentClassDeclaration = currentClassDeclaration.AddCall(expression, loggerFieldDeclaration, rowNumber, columnNumber, ref currentDocument);
            CurrentProject = currentDocument.Project;
        }
    }
}
