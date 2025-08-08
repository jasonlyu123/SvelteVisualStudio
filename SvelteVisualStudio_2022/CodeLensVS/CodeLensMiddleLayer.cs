using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;
using SvelteVisualStudio.MiddleLayers;
using System;
using System.Threading.Tasks;

namespace SvelteVisualStudio_2022.CodeLensVS
{
    internal class CodeLensMiddleLayer : IMiddleLayerProvider
    {
        public string Method => Methods.TextDocumentCodeLensName;

        public Task HandleNotificationAsync(JToken methodParam, Func<JToken, Task> sendNotification)
        {
            throw new NotImplementedException();
        }

        public Task<JToken> HandleRequestAsync(JToken methodParam, Func<JToken, Task<JToken>> sendRequest)
        {
            return Task.FromResult(JToken.FromObject(Array.Empty<CodeLens[]>()));
        }
    }
}
