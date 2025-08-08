using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;
using SvelteVisualStudio.MiddleLayers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SvelteVisualStudio_2022.Document
{
    internal class LspOpenedDocumentManager
    {
        public event EventHandler<Uri> DocumentOpen;
        private event EventHandler<Uri> DocumentClose;

        private readonly IMiddleLayerProvider trackOpen;
        private readonly IMiddleLayerProvider trackClose;
        private readonly HashSet<Uri> currentDocuments = new HashSet<Uri>();

        public LspOpenedDocumentManager(MiddleLayerHost host) 
        {
            trackOpen = new Tracker(Methods.TextDocumentDidOpenName, this);
            trackClose = new Tracker(Methods.TextDocumentDidCloseName, this);
            host.Register(trackOpen);
            host.Register(trackClose);

            DocumentOpen += OpenedDocumentManager_DocumentOpen;
            DocumentClose += OpenedDocumentManager_DocumentClose;
        }

        public bool Has(Uri uri) 
        {
            return currentDocuments.Contains(uri);
        }

        private void OpenedDocumentManager_DocumentClose(object sender, Uri e)
        {
            currentDocuments.Remove(e);
        }

        private void OpenedDocumentManager_DocumentOpen(object sender, Uri e)
        {
            currentDocuments.Add(e);
        }

        private class Tracker : IMiddleLayerProvider
        {
            private readonly string method;
            private readonly LspOpenedDocumentManager manager;

            public Tracker(string method, LspOpenedDocumentManager manager)
            {
                this.method = method;
                this.manager = manager;
            }

            public string Method => method;

            public async Task HandleNotificationAsync(JToken methodParam, Func<JToken, Task> sendNotification)
            {
                await sendNotification(methodParam);
                if (method == Methods.TextDocumentDidOpenName)
                {
                    var param = methodParam.ToObject<DidCloseTextDocumentParams>();
                    manager.DocumentOpen.Invoke(null, param.TextDocument.Uri);
                }
                else if (method == Methods.TextDocumentDidCloseName)
                {
                    var param = methodParam.ToObject<DidCloseTextDocumentParams>();
                    manager.DocumentClose.Invoke(null, param.TextDocument.Uri);
                }
            }

            public Task<JToken> HandleRequestAsync(JToken methodParam, Func<JToken, Task<JToken>> sendRequest)
            {
                throw new NotImplementedException();
            }
        }
    }
}
