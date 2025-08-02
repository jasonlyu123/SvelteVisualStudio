using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.CodeLens;
using System.Threading;
using Microsoft.VisualStudio.Threading;

namespace SvelteVisualStudio.CodeLens
{
    [Export(typeof(IAsyncCodeLensDataPointProvider))]
    [Name(Id)]
    [ContentType("code")]
    //[LocalizedName(typeof(FeaturesResources), nameof(FeaturesResources.CSharp_VisualBasic_References))]
    //[Priority(200)]
    //[OptionUserModifiable(userModifiable: false)]
    [DetailsTemplateName("references")]
    internal sealed class ReferenceCodeLensProvider : IAsyncCodeLensDataPointProvider 
    { 
        private const string Id = "SvelteReferences";

        public Task<bool> CanCreateDataPointAsync(CodeLensDescriptor descriptor, CodeLensDescriptorContext descriptorContext, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        public Task<IAsyncCodeLensDataPoint> CreateDataPointAsync(CodeLensDescriptor descriptor, CodeLensDescriptorContext descriptorContext, CancellationToken token)
        {
            return Task.FromResult<IAsyncCodeLensDataPoint>(new CodeLensData(descriptor, descriptorContext));
        }

        private class CodeLensData : IAsyncCodeLensDataPoint
        {
            private readonly CodeLensDescriptorContext descriptorContext;
            private readonly CodeLensDescriptor descriptor;

            public CodeLensData(CodeLensDescriptor descriptor, CodeLensDescriptorContext descriptorContext)
            {
                this.descriptor = descriptor;
                this.descriptorContext = descriptorContext;
            }

            public CodeLensDescriptor Descriptor => descriptor;

            public event AsyncEventHandler InvalidatedAsync;

            public Task<CodeLensDataPointDescriptor> GetDataAsync(CodeLensDescriptorContext descriptorContext, CancellationToken token)
            {
                return Task.FromResult(new CodeLensDataPointDescriptor
                {
                    Description = "References",
                });
            }

            public Task<CodeLensDetailsDescriptor> GetDetailsAsync(CodeLensDescriptorContext descriptorContext, CancellationToken token)
            {
                return Task.FromResult(new CodeLensDetailsDescriptor
                {
                    Entries = Array.Empty<CodeLensDetailEntryDescriptor>()
                });
            }
        }
    }
}