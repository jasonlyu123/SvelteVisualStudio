using Microsoft.VisualStudio.Shell;
using System;

namespace SvelteVisualStudio.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class ProvideConfigAttribute : RegistrationAttribute
    {
        private readonly string configName;
        private readonly string scope;
        private readonly string value;

        public ProvideConfigAttribute(string configName, string scope, string value)
        {
            this.configName = configName;
            this.scope = scope;
            this.value = value;
        }

        public override void Register(RegistrationContext context)
        {
            using (var key = context.CreateKey(configName))
            {
                key.SetValue(scope, value);
            }
        }

        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey(configName);
        }
    }
}
