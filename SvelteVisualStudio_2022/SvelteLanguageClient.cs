using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
using SvelteVisualStudio;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace SvelteVisualStudio_2022
{
    [ContentType(SvelteContentDefinition.Identifier)]
    [Export(typeof(ILanguageClient))]
    // The ILanguageClientCustomMessage used in the official LSP docs example doesn't work anymore
    class SvelteLanguageClient : SvelteLanguageClientBase, ILanguageClient
    {
        [ImportingConstructor]
        public SvelteLanguageClient(
            [Import] IVsFolderWorkspaceService workspaceService) : base(workspaceService)
        { }

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
