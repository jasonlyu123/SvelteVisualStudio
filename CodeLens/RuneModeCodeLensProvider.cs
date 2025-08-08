using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.CodeLens;
using System.Threading;
using Microsoft.VisualStudio.Threading;
using System.Linq;

namespace SvelteVisualStudio.CodeLens
{
    [Export(typeof(IAsyncCodeLensDataPointProvider))]
    [Name(Id)]
    [ContentType("svelte")]
    //[LocalizedName(typeof(FeaturesResources), nameof(FeaturesResources.CSharp_VisualBasic_References))]
    //[Priority(200)]
    //[OptionUserModifiable(userModifiable: false)]
    //[DetailsTemplateName("references")]
    internal sealed class RunModeCodeLensProvider : IAsyncCodeLensDataPointProvider 
    { 
        private const string Id = "Svelte rune mode indicator";
        private const string type = "rune";

        public Task<bool> CanCreateDataPointAsync(CodeLensDescriptor descriptor, CodeLensDescriptorContext descriptorContext, CancellationToken token)
        {
            var list = CodeLensUtils.GetCodeLensInfos(descriptorContext, type);
            return Task.FromResult(list.Length > 0);
        }

        public Task<IAsyncCodeLensDataPoint> CreateDataPointAsync(CodeLensDescriptor descriptor, CodeLensDescriptorContext descriptorContext, CancellationToken token)
        {
            return Task.FromResult<IAsyncCodeLensDataPoint>(new CodeLensData(descriptor));
        }

        private class CodeLensData : IAsyncCodeLensDataPoint
        {
            private readonly CodeLensDescriptor descriptor;

            public CodeLensData(CodeLensDescriptor descriptor)
            {
                this.descriptor = descriptor;
            }

            public CodeLensDescriptor Descriptor => descriptor;

            public event AsyncEventHandler InvalidatedAsync
            {
                add { }
                remove { }
            }

            public Task<CodeLensDataPointDescriptor> GetDataAsync(CodeLensDescriptorContext descriptorContext, CancellationToken token)
            {
                var list = CodeLensUtils.GetCodeLensInfos(descriptorContext, type);
                var codelens = list.FirstOrDefault();
                if (codelens is null)
                {
                    return Task.FromResult(new CodeLensDataPointDescriptor());
                }
                return Task.FromResult(new CodeLensDataPointDescriptor
                {
                    Description = codelens.Title,
                    TooltipText = ""
                });
            }

            public Task<CodeLensDetailsDescriptor> GetDetailsAsync(CodeLensDescriptorContext descriptorContext, CancellationToken token)
            {
                return Task.FromResult(new CodeLensDetailsDescriptor
                {
                    //PaneNavigationCommands = new[] {
                });
            }
        }
    }
}