using Microsoft.VisualStudio.LanguageServer.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvelteVisualStudio.MiddleLayers
{
    class MiddleLayerHost : ILanguageClientMiddleLayer
    {
        private readonly List<IMiddleLayerProvider> providers = new List<IMiddleLayerProvider>();

        public void Register(IMiddleLayerProvider middleLayerProvider)
        {
            providers.Add(middleLayerProvider);
        }

        public bool CanHandle(string methodName)
        {
            return providers.Any(Finder(methodName));
        }

        private static Func<IMiddleLayerProvider, bool> Finder(string methodName)
        {
            return provider => provider.Method == methodName;
        }

        private IMiddleLayerProvider Find(string methodName)
        {
            return providers.SingleOrDefault(Finder(methodName));
        }

        public Task HandleNotificationAsync(string methodName, JToken methodParam, Func<JToken, Task> sendNotification)
        {
            return Find(methodName)?.HandleNotificationAsync(methodParam, sendNotification) ?? Task.CompletedTask;
        }

        public Task<JToken> HandleRequestAsync(string methodName, JToken methodParam, Func<JToken, Task<JToken>> sendRequest)
        {
            return Find(methodName)?.HandleRequestAsync(methodParam, sendRequest) ??
                Task.FromResult<JToken>(null);
        }
    }
}
