using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace SvelteVisualStudio
{
    static class SvelteContentDefinition
    {
        public const string Identifier = "svelte";
        private const string extension = ".svelte";

        [Export]
        [Name(Identifier)]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static ContentTypeDefinition SvelteContentTypeDefinition;

        [Export]
        [FileExtension(extension)]
        [ContentType(Identifier)]
        internal static FileExtensionToContentTypeDefinition SvelteFileExtensionDefinition;
    }
}
