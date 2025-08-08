using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using SvelteVisualStudio;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

namespace SvelteVisualStudio_2022.Document
{
    [Export(typeof(ITextViewConnectionListener))]
    [Export(typeof(DocumentTextBufferManager))]
    [ContentType("Typescript")]
    [ContentType(SvelteContentDefinition.Identifier)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class DocumentTextBufferManager : ITextViewConnectionListener
    {
        private readonly Dictionary<string, ITextDocument> documents = new Dictionary<string, ITextDocument>(
            StringComparer.OrdinalIgnoreCase);
        private readonly ITextBufferFactoryService textBufferFactoryService;

        public ITextBuffer GetOrCreate(string filePath)
        {
            filePath = NormalizePath(Path.GetFullPath(filePath));
            if (documents.TryGetValue(filePath, out var document))
            {
                return document.TextBuffer;
            }

            if (!File.Exists(filePath))
            {
                return null;
            }
            var text = File.ReadAllText(filePath);

            return textBufferFactoryService.CreateTextBuffer(text, textBufferFactoryService.PlaintextContentType);
        }

        [ImportingConstructor]
        public DocumentTextBufferManager([Import] ITextBufferFactoryService textBufferFactoryService)
        {
            this.textBufferFactoryService = textBufferFactoryService;
        }

        public void SubjectBuffersConnected(
            ITextView textView,
            ConnectionReason reason,
            IReadOnlyCollection<ITextBuffer> subjectBuffers)
        {
            var type = typeof(ITextBuffer);
            foreach (var buffer in subjectBuffers)
            {
                if (
                    buffer.Properties.TryGetProperty(type, out ITextDocument textDocument) &&
                    textDocument != null &&
                    !documents.ContainsKey(textDocument.FilePath))
                {
                    documents.Add(NormalizePath(textDocument.FilePath), textDocument);
                }
            }
        }

        private static string NormalizePath(string path)
        {
            return path.Replace("\\", "/");
        }

        public void SubjectBuffersDisconnected(
            ITextView textView,
            ConnectionReason reason,
            IReadOnlyCollection<ITextBuffer> subjectBuffers)
        {
            var type = typeof(ITextBuffer);
            foreach (var buffer in subjectBuffers)
            {
                if (
                    buffer.Properties.TryGetProperty(type, out ITextDocument textDocument) &&
                    textDocument != null)
                {
                    documents.Remove(NormalizePath(textDocument.FilePath));
                }
            }
        }
    }
}
