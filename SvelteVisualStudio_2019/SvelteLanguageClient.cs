using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
using SvelteVisualStudio;
using System.ComponentModel.Composition;

namespace SvelteVisualStudio_2019
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
    }
}
