using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SvelteVisualStudio.CodeLens
{
    [Export(typeof(IAsyncCodeLensDataPointProvider))]
    [Name(Id)]
    [ContentType("svelte")]
    [Priority(200)]
    [DetailsTemplateName("references")]
    internal sealed class ReferenceCodeLensProvider : ReferenceCodeLensProviderBase
    {
        private const string Id = "Svelte References CodeLens";
        private const string type = "reference";

        [ImportingConstructor]
        public ReferenceCodeLensProvider([Import] ICodeLensCallbackService codeLensCallbackService) : base(codeLensCallbackService)
        {
        }

        protected override string TargetType => type;
    }

    [Export(typeof(IAsyncCodeLensDataPointProvider))]
    [Name(Id)]
    [ContentType("svelte")]
    [Priority(201)]
    [DetailsTemplateName("references")]
    internal sealed class ImplementaionCodeLensProvider : ReferenceCodeLensProviderBase
    {
        private const string Id = "Svelte Implementation CodeLens";
        private const string type = "implementation";

        [ImportingConstructor]
        public ImplementaionCodeLensProvider([Import] ICodeLensCallbackService codeLensCallbackService) : base(codeLensCallbackService)
        {
        }

        protected override string TargetType => type;
    }

    internal class ReferenceCodeLensProviderBase : IAsyncCodeLensDataPointProvider
    {
        private readonly ICodeLensCallbackService codeLensCallbackService;

        public ReferenceCodeLensProviderBase(ICodeLensCallbackService codeLensCallbackService)
        {
            this.codeLensCallbackService = codeLensCallbackService;
        }

        protected virtual string TargetType { get; }

        public Task<bool> CanCreateDataPointAsync(CodeLensDescriptor descriptor, CodeLensDescriptorContext descriptorContext, CancellationToken token)
        {
            var list = CodeLensUtils.GetCodeLensInfos(descriptorContext, TargetType);
            return Task.FromResult(list.Length > 0);
        }

        public Task<IAsyncCodeLensDataPoint> CreateDataPointAsync(CodeLensDescriptor descriptor, CodeLensDescriptorContext descriptorContext, CancellationToken token)
        {
            return Task.FromResult<IAsyncCodeLensDataPoint>(new CodeLensData(descriptor, TargetType, codeLensCallbackService));
        }

        private class CodeLensData : IAsyncCodeLensDataPoint
        {
            private readonly CodeLensDescriptor descriptor;
            private readonly string targetType;
            private readonly ICodeLensCallbackService codeLensCallbackService;

            public CodeLensData(CodeLensDescriptor descriptor, string targetType, ICodeLensCallbackService codeLensCallbackService)
            {
                this.descriptor = descriptor;
                this.targetType = targetType;
                this.codeLensCallbackService = codeLensCallbackService;
            }

            private static readonly List<CodeLensDetailHeaderDescriptor> s_header = new List<CodeLensDetailHeaderDescriptor>()
            {
                new CodeLensDetailHeaderDescriptor() { UniqueName = ReferenceEntryFieldNames.FilePath },
                new CodeLensDetailHeaderDescriptor() { UniqueName = ReferenceEntryFieldNames.LineNumber },
                new CodeLensDetailHeaderDescriptor() { UniqueName = ReferenceEntryFieldNames.ColumnNumber },
                new CodeLensDetailHeaderDescriptor() { UniqueName = ReferenceEntryFieldNames.ReferenceText },
                new CodeLensDetailHeaderDescriptor() { UniqueName = ReferenceEntryFieldNames.ReferenceStart },
                new CodeLensDetailHeaderDescriptor() { UniqueName = ReferenceEntryFieldNames.ReferenceEnd },
                //new CodeLensDetailHeaderDescriptor() { UniqueName = ReferenceEntryFieldNames.ReferenceLongDescription },
                //new CodeLensDetailHeaderDescriptor() { UniqueName = ReferenceEntryFieldNames.ReferenceImageId },
                new CodeLensDetailHeaderDescriptor() { UniqueName = ReferenceEntryFieldNames.TextBeforeReference2 },
                new CodeLensDetailHeaderDescriptor() { UniqueName = ReferenceEntryFieldNames.TextBeforeReference1 },
                new CodeLensDetailHeaderDescriptor() { UniqueName = ReferenceEntryFieldNames.TextAfterReference1 },
                new CodeLensDetailHeaderDescriptor() { UniqueName = ReferenceEntryFieldNames.TextAfterReference2 },
            };

            public CodeLensDescriptor Descriptor => descriptor;

            public event AsyncEventHandler InvalidatedAsync
            {
                add { }
                remove { }
            }

            public Task<CodeLensDataPointDescriptor> GetDataAsync(CodeLensDescriptorContext descriptorContext, CancellationToken token)
            {
                var reference = CodeLensUtils.GetCodeLensInfos(descriptorContext, targetType).FirstOrDefault();
                if (reference is null)
                {
                    return Task.FromResult(new CodeLensDataPointDescriptor());
                }
                return Task.FromResult(new CodeLensDataPointDescriptor
                {
                    Description = reference.Title,
                    TooltipText = "This symbol has " + reference.Title
                });
            }

            public async Task<CodeLensDetailsDescriptor> GetDetailsAsync(CodeLensDescriptorContext descriptorContext, CancellationToken token)
            {
                var referenceInfo = CodeLensUtils.GetCodeLensInfos(descriptorContext, targetType).FirstOrDefault();
                var references = await codeLensCallbackService.InvokeAsync<CodelensReferenceInfoFull[]>(
                    this,
                    "ConvertReferenceLocations",
                    new object[] { referenceInfo.References }
                );
                return new CodeLensDetailsDescriptor
                {
                    Headers = s_header,
                    Entries = references.Select(referenceLocationDescriptor => new CodeLensDetailEntryDescriptor
                    {
                        Fields = new[]
                        {
                            new CodeLensDetailEntryField() { Text = referenceLocationDescriptor.FilePath },
                            new CodeLensDetailEntryField() { Text = referenceLocationDescriptor.LineNumber.ToString() },
                            new CodeLensDetailEntryField() { Text = referenceLocationDescriptor.ColumnNumber.ToString() },
                            new CodeLensDetailEntryField() { Text = referenceLocationDescriptor.ReferenceLineText },
                            new CodeLensDetailEntryField() { Text = referenceLocationDescriptor.ReferenceStart.ToString() },
                            new CodeLensDetailEntryField() { Text = (referenceLocationDescriptor.ReferenceStart + referenceLocationDescriptor.ReferenceLength).ToString() },
                            new CodeLensDetailEntryField() { Text = referenceLocationDescriptor.BeforeReferenceText2 },
                            new CodeLensDetailEntryField() { Text = referenceLocationDescriptor.BeforeReferenceText1 },
                            new CodeLensDetailEntryField() { Text = referenceLocationDescriptor.AfterReferenceText1 },
                            new CodeLensDetailEntryField() { Text = referenceLocationDescriptor.AfterReferenceText2 }
                        }
                    })
                };
            }
        }
    }
}