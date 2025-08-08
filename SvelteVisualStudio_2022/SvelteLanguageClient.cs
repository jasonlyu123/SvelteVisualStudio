using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
using StreamJsonRpc;
using SvelteVisualStudio;
using SvelteVisualStudio.MiddleLayers;
using SvelteVisualStudio_2022.CodeLensVS;
using SvelteVisualStudio_2022.Document;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace SvelteVisualStudio_2022
{
    [ContentType(SvelteContentDefinition.Identifier)]
    [Export(typeof(ILanguageClient))]
    class SvelteLanguageClient : SvelteLanguageClientBase, ILanguageClient, IJsonRpcAccessor
    {
        [ImportingConstructor]
        public SvelteLanguageClient(
            [Import] IVsFolderWorkspaceService workspaceService,
            [Import] TsJsTextBufferManager tsJsTextBufferManager,
            [Import] CodeLensProvider codeLensTaggerProvider)
                : base(workspaceService, tsJsTextBufferManager)
        {
            var documentManager = new LspOpenedDocumentManager(middleLayerHost);
            middleLayerHost.Register(new CompletionMiddleLayer());
            middleLayerHost.Register(new CodeLensMiddleLayer());
            codeLensTaggerProvider.Registers(this, documentManager);
        }

        public bool ShowNotificationOnInitializeFailed => true;
        public event AsyncEventHandler<EventArgs> StopAsync
        {
            add { }
            remove { }
        }

        public object InitializationOptions => new
        {
            shouldFilterCodeActionKind = true,
            configuration = new
            {
                typescript = new
                {
                    referencesCodeLens = new {  enabled = true }
                }
            }
        };

        public Task<InitializationFailureContext> OnServerInitializeFailedAsync(
            ILanguageClientInitializationInfo initializationState)
        {
            return Task.FromResult(new InitializationFailureContext()
            {
                FailureMessage = $"Language server failed to initialize during the {initializationState.Status} Stage"
            });
        }

        public JsonRpc GetJsonRpc()
        {
            return rpc;
        }
    }
}
