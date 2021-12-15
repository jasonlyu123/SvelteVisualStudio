using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
using SvelteVisualStudio;
using SvelteVisualStudio.MiddleLayers;
using System;
using System.ComponentModel.Composition;

namespace SvelteVisualStudio_2019
{
    [ContentType(SvelteContentDefinition.Identifier)]
    [Export(typeof(ILanguageClient))]
    // The ILanguageClientCustomMessage used in the official LSP docs example doesn't work anymore
    class SvelteLanguageClient : SvelteLanguageClientBase, ILanguageClient
    {
        public event AsyncEventHandler<EventArgs> StopAsync 
        {
            add { }
            remove { }
        }

        [ImportingConstructor]
        public SvelteLanguageClient(
            [Import] IVsFolderWorkspaceService workspaceService,
            [Import] TsJsTextBufferManager tsJsTextBufferManager) 
                : base(workspaceService, tsJsTextBufferManager)
        {
            middleLayerHost.Register(new CompletionMiddleLayer(shouldFilterOutJSDocSnippet: true));
        }
    }
}
