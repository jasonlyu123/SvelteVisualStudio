using Microsoft.VisualStudio.Shell;
using System;

namespace SvelteVisualStudio.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class ProvideTextMateGrammarAttribute : RegistrationAttribute
    {
        private const string defaultDirectory = "$PackageFolder$\\Grammars";
        private const string keyName = "TextMate\\Repositories";

        public ProvideTextMateGrammarAttribute(string key, string directory = defaultDirectory)
        {
            Scope = key;
            Directory = directory;
        }

        public string Scope { get; }
        public string Directory { get; }

        public override void Register(RegistrationContext context)
        {
            using (var key = context.CreateKey(keyName))
            {
                key.SetValue(Scope, Directory);
            }
        }

        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey(keyName);
        }
    }
}
