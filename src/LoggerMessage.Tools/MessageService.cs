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

            var projectDir = Path.GetDirectoryName(currentProject.FilePath);
            //if (!Directory.Exists(Path.Combine(projectDir, Constants.LoggerMessagesFolderName)))
            //    Directory.CreateDirectory(Path.Combine(projectDir, Constants.LoggerMessagesFolderName));

            if (!Directory.Exists(Path.Combine(projectDir, Constants.LoggerMessagesResxFolderName)))
                Directory.CreateDirectory(Path.Combine(projectDir, Constants.LoggerMessagesResxFolderName));

            GetOrCreateMessagesResx(CurrentProject);

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

        public void AddMessageToResource(MessageMethod message)
        {
            CurrentProject.AddOrUpdateResource(message);

            CurrentProject.GenerateResxClass();
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

        public static void CreateMessagesResx(string resxFilePath)
        {
            using (ResXResourceWriter resx = new ResXResourceWriter(resxFilePath))
            {
                resx.Generate();
                resx.Close();
            }
        }

        public void GetOrCreateMessagesResx(Project project)
        {
            var resxFilePth = project.GetResxPath();

            if (!File.Exists(resxFilePth))
                CreateMessagesResx(resxFilePth);
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


        public MessageMethod GetLoggerMessage(int rowNumber, int columnNumber)
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

            var loggerVariable = _currentClassDeclaration.GetOrCreateLoggerVariableName(model, out loggerFieldDeclaration);

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
