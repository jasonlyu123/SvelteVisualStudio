using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace SvelteVisualStudio
{
    [Export(typeof(ITextViewConnectionListener))]
    [Export(typeof(TsJsTextBufferManager))]
    // Js seems to also uses this content type
    [ContentType("Typescript")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class TsJsTextBufferManager : ITextViewConnectionListener
    {
        private readonly HashSet<ITextBuffer> buffers = new HashSet<ITextBuffer>();

        public IReadOnlyCollection<ITextBuffer> Buffers => buffers;

        public delegate void BufferRegisterationHandler(ITextBuffer buffer);
        public event BufferRegisterationHandler BufferRegistered;
        public event BufferRegisterationHandler BufferUnRegistered;

        public void SubjectBuffersConnected(
            ITextView textView,
            ConnectionReason reason,
            IReadOnlyCollection<ITextBuffer> subjectBuffers)
        {
            foreach (var buffer in subjectBuffers)
            {
                buffers.Add(buffer);
                BufferRegistered?.Invoke(buffer);
            }
        }

        public void SubjectBuffersDisconnected(
            ITextView textView,
            ConnectionReason reason,
            IReadOnlyCollection<ITextBuffer> subjectBuffers)
        {
            foreach (var buffer in subjectBuffers)
            {
                buffers.Remove(buffer);
                BufferUnRegistered?.Invoke(buffer);
            }
        }
    }
}
