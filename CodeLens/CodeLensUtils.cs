using Microsoft.VisualStudio.Language.CodeLens;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace SvelteVisualStudio.CodeLens
{
    internal static class CodeLensUtils
    {
        private const string valuesKey = "Values";
        public static CodeLensInfo[] GetCodeLensInfos(
            CodeLensDescriptorContext descriptorContext,
            string type)
        {
            if (!descriptorContext.Properties.TryGetValue(valuesKey, out var values))
            {
                return Array.Empty<CodeLensInfo>();
            }

            return (values as JArray)?.ToObject<CodeLensInfo[]>().Where(v => v.Type == type).ToArray()
                ?? Array.Empty<CodeLensInfo>();
        }
    }
}
