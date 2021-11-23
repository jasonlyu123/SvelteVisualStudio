using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
using SvelteVisualStudio;
using SvelteVisualStudio.MiddleLayers;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace SvelteVisualStudio_2022
{
    [ContentType(SvelteContentDefinition.Identifier)]
    [Export(typeof(ILanguageClient))]
    class SvelteLanguageClient : SvelteLanguageClientBase, ILanguageClient
    {
        [ImportingConstructor]
        public SvelteLanguageClient(
            [Import] IVsFolderWorkspaceService workspaceService) : base(workspaceService)
        {
            middleLayerHost.Register(new CompletionMiddleLayer(shouldFilterOutJSDocSnippet: true));
        }

        public bool ShowNotificationOnInitializeFailed => true;
        public event AsyncEventHandler<EventArgs> StopAsync
        {
            add { }
            remove { }
        }

        public Task<InitializationFailureContext> OnServerInitializeFailedAsync(
            ILanguageClientInitializationInfo initializationState)
        {
            return Task.FromResult(new InitializationFailureContext() 
            {
                FailureMessage = $"Language server failed to initialize during the {initializationState.Status} Stage"
            });
        }
    }
}
