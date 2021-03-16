using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace SvelteVisualStudio
{
    static class SvelteContentDefinition
    {
        public const string Identifier = "svelte";
        public const string Extension = ".svelte";

        [Export]
        [Name(Identifier)]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        //[BaseDefinition("htmlx")]
        internal static ContentTypeDefinition SvelteContentTypeDefinition;

        [Export]
        [FileExtension(Extension)]
        [ContentType(Identifier)]
        internal static FileExtensionToContentTypeDefinition SvelteFileExtensionDefinition;
    }
}
