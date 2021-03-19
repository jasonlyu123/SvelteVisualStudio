using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SvelteVisualStudio.MiddleLayers
{
    class CompletionMiddleLayer : IMiddleLayerProvider
    {
        public string Method => "textDocument/completion";

        public Task HandleNotificationAsync(JToken methodParam, Func<JToken, Task> sendNotification)
        {
            throw new NotImplementedException();
        }

        public async Task<JToken> HandleRequestAsync(JToken methodParam, Func<JToken, Task<JToken>> sendRequest)
        {
            var res = await sendRequest(methodParam);

            var completion = res.ToObject<CompletionList>();

            var filtered = new List<CompletionItem>();
            foreach (var item in completion.Items)
            {
                // JSDoc template only works when there's block comment auto close
                // otherwise, the next token would be treated as a comment
                // thus resulting a wrong result
                if (item.Label == "/** */")
                {
                    continue;
                }

                if (item.InsertTextFormat != InsertTextFormat.Snippet)
                {
                    filtered.Add(item);
                    continue;
                }

                var processed = TryRemoveSnippet(item);
                if (processed != null)
                {
                    processed.InsertTextFormat = InsertTextFormat.Plaintext;
                    filtered.Add(processed);
                }
            }

            res["items"].Replace(JArray.FromObject(filtered));

            return res;
        }

        private CompletionItem TryRemoveSnippet(CompletionItem item)
        {
            if (!string.IsNullOrEmpty(item.TextEdit?.NewText))
            {
                if (TryRemoveSnippet(item.TextEdit.NewText, out var result))
                {
                    item.TextEdit.NewText = result;
                    return item;
                }
            }
            else if (!string.IsNullOrEmpty(item.InsertText))
            {
                if (TryRemoveSnippet(item.InsertText, out var result))
                {
                    item.InsertText = result;
                    return item;
                }
            }
            else if (TryRemoveSnippet(item.Label, out var result))
            {
                item.Label = result;
                return item;
            }

            return null;
        }

        private readonly Regex tabStop = new Regex(@"(\\\$)|(\$[0-9]+|\${[0-9]+})");
        private readonly Regex placeHolder = new Regex(@"(\\\$)|\${[0-9]+:(.*)?}");
        private readonly Regex notEscaped = new Regex(@"\$(?<!\\\$)");

        /// <summary>
        /// remove simple snippet, if it's too complicated filter it out.
        /// </summary>
        private bool TryRemoveSnippet(string input, out string result)
        {
            result = input;
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            if (tabStop.IsMatch(result))
            {
                result = tabStop.Replace(result, match => match.Groups[1].Value);
            }
            if (placeHolder.IsMatch(result))
            {
                result = placeHolder.Replace(result, match =>
                {
                    var escaped = match.Groups[1].Value;
                    return string.IsNullOrEmpty(escaped) ? match.Groups[2].Value : escaped;
                });
            }

            var stillHasDollarSign = notEscaped.IsMatch(result);
            if (!stillHasDollarSign)
            {
                result = result.Replace(@"\$", "$");
            }

            return !stillHasDollarSign;
        }
    }
}
