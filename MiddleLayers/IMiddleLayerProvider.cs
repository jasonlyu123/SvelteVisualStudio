using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace SvelteVisualStudio.MiddleLayers
{
    interface IMiddleLayerProvider
    {
        string Method { get; }

        Task<JToken> HandleRequestAsync(JToken methodParam, Func<JToken, Task<JToken>> sendRequest);

        Task HandleNotificationAsync(JToken methodParam, Func<JToken, Task> sendNotification);
    }
}
