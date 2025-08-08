using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json.Linq;
using SvelteVisualStudio_2022.Document;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SvelteVisualStudio_2022.CodeLensVS
{
    [Export(typeof(ITaggerProvider))]
    [Export(typeof(CodeLensProvider))]
    [Export(typeof(ICodeLensCallbackListener))]
    [TagType(typeof(ICodeLensTag3))]
    [ContentType("svelte")]
    internal class CodeLensProvider : ITaggerProvider, ICodeLensCallbackListener
    {
        [ImportingConstructor]
        public CodeLensProvider(
            [Import] DocumentTextBufferManager textBufferManager)
        {
            this.textBufferManager = textBufferManager;
        }

        private IJsonRpcAccessor jsonRpcAccessor;
        private LspOpenedDocumentManager documentManager;
        private readonly DocumentTextBufferManager textBufferManager;
        private readonly Dictionary<Uri, Func<Task>> pending = new Dictionary<Uri, Func<Task>>();

        public void Registers(IJsonRpcAccessor accessor, LspOpenedDocumentManager openedDocumentManager)
        {
            jsonRpcAccessor = accessor;
            documentManager = openedDocumentManager;
            documentManager.DocumentOpen += DocumentManager_DocumentOpen;
        }

        private void DocumentManager_DocumentOpen(object sender, Uri e)
        {
            if (!pending.TryGetValue(e, out var pendingRefresh))
                return;

            _ = pendingRefresh.Invoke();
            pending.Remove(e);
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITagger<T> sc()
            {
                return new CodeLensTagger(buffer, this) as ITagger<T>;
            }
            return buffer.Properties.GetOrCreateSingletonProperty(sc);
        }

        public ImmutableArray<CodelensReferenceInfoFull> ConvertReferenceLocations(
            CodelensReferenceInfo[] references)
        {
            var result = new List<CodelensReferenceInfoFull>();

            foreach (var item in references)
            {
                var buffer = textBufferManager.GetOrCreate(item.FilePath);
                if (buffer == null)
                {
                    continue;
                }
                var endOffset = OffsetAt(buffer, new Position
                {
                    Line = item.LineNumber,
                    Character = item.ColumnNumber
                });
                var startOffset = OffsetAt(buffer, new Position
                {
                    Line = item.LineNumber,
                    Character = item.ColumnNumber
                });

                var snapshot = buffer.CurrentSnapshot;
                var line = snapshot.GetLineFromPosition(startOffset);

                result.Add(new CodelensReferenceInfoFull
                {
                    LineNumber = item.LineNumber,
                    ColumnNumber = item.ColumnNumber,
                    FilePath = item.FilePath,
                    ReferenceStart = startOffset - line.Start.Position,
                    ReferenceLength = endOffset - startOffset,
                    ReferenceLineText = line.GetText(),
                    AfterReferenceText1 = GetLineText(line.LineNumber + 1, snapshot),
                    AfterReferenceText2 = GetLineText(line.LineNumber + 2, snapshot),
                    BeforeReferenceText1 = GetLineText(line.LineNumber - 1, snapshot),
                    BeforeReferenceText2 = GetLineText(line.LineNumber - 2, snapshot),
                });
            }

            return result.ToImmutableArray();
        }

        private string GetLineText(int lineNumber, ITextSnapshot snapshot)
        {
            if (lineNumber < 0 || lineNumber >= snapshot.LineCount)
                return null;

            return snapshot.GetLineFromLineNumber(lineNumber).GetText();
        }

        private void ScheduleRefresh(Uri uri, Func<Task> refreshAsync) 
        {
            if (pending.ContainsKey(uri))
            {
                return;
            }
            if (documentManager is null || !documentManager.Has(uri))
            {
                pending.Add(uri, refreshAsync);
            }
            else 
            {
                _ = refreshAsync();
            }
        }

        private async Task<CodeLens[]> QueryAsync(Uri uri, CancellationToken cancellationToken)
        {
            var jsonRpc = jsonRpcAccessor.GetJsonRpc();
            if (jsonRpc is null)
            {
                return Array.Empty<CodeLens>();
            }

            var codelensParams = new CodeLensParams
            {
                TextDocument = new TextDocumentIdentifier
                {
                    Uri = uri
                }
            };

            var codelens = await jsonRpc.InvokeWithParameterObjectAsync<CodeLens[]>(
                    Methods.TextDocumentCodeLensName,
                    codelensParams,
                    cancellationToken
             );

            return codelens;
        }

        private async Task<CodeLens> ResolveAsync(CodeLens codeLens)
        {
            var jsonRpc = jsonRpcAccessor.GetJsonRpc();
            if (jsonRpc is null)
            {
                return codeLens;
            }

            var resolvedCodeLens = await jsonRpc.InvokeWithParameterObjectAsync<CodeLens>(
                    Methods.CodeLensResolveName,
                    codeLens
             );

            return resolvedCodeLens;
        }

        private static int OffsetAt(ITextBuffer buffer, Position position)
        {
            return buffer.CurrentSnapshot.GetLineFromLineNumber(position.Line).Start.Position + position.Character;
        }

        private static string UriToPath(Uri uri)
        {
            var path = uri.LocalPath;
            if (!path.StartsWith("/"))
                return path;

            var parts = path.Split('/');
            if (parts.Length > 1 && parts[1].EndsWith(":"))
            {
                return path.Substring(1);
            }
            return path;
        }

        private class CodeLensTagger : ITagger<ICodeLensTag3>
        {
            private readonly ITextBuffer buffer;
            private readonly string filePath;
            private readonly CodeLensProvider provider;
            private SvelteCodeLensTag[] result;
            private CancellationTokenSource cancellationTokenSource;

            public CodeLensTagger(ITextBuffer buffer, CodeLensProvider provider)
            {
                this.buffer = buffer;
                this.provider = provider;
                this.buffer.ChangedLowPriority += Buffer_ChangedLowPriority;

                if (
                    buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument textDocument) &&
                    textDocument != null)
                {
                    filePath = textDocument.FilePath;
                    this.provider.ScheduleRefresh(new Uri(filePath), this.RefreshAsync);
                }
            }

            private void Buffer_ChangedLowPriority(object sender, TextContentChangedEventArgs e)
            {
                _ = RefreshAsync();
            }

            public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

            public IEnumerable<ITagSpan<ICodeLensTag3>> GetTags(NormalizedSnapshotSpanCollection spans)
            {
                if (result == null)
                {
                    return Array.Empty<ITagSpan<ICodeLensTag3>>();
                }
                var resultForSpan = result.Where(tag => spans.Any(v => v.IntersectsWith(tag.TextSpan)));

                return resultForSpan.Select(v => new TagSpan<SvelteCodeLensTag>(
                    new SnapshotSpan(buffer.CurrentSnapshot, v.TextSpan),
                    v
                 ));
            }

            private async Task RefreshAsync()
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                var uri = new Uri(filePath);
                var lspResult = await provider.QueryAsync(uri, cancellationTokenSource.Token);

                var newResult = new List<SvelteCodeLensTag>();
                var added = new List<SvelteCodeLensTag>();

                var groupsByLine = lspResult.GroupBy(v => v.Range.Start.Line);
                var withSpan = groupsByLine.SelectMany(lineGroup => lineGroup.Select((codeLens, i) =>
                    (codeLens,
                    span: ConvertSpan(codeLens.Range, mapToStart: i == 0))
                 )).ToArray();

                var exists = result?.ToDictionary(v => v.TextSpan.Start) ?? new Dictionary<int, SvelteCodeLensTag>();
                var removing = exists.ToDictionary(v => v.Key, v => v.Value);
                var deferReplace = new List<SvelteCodeLensTag>();
                foreach (var group in withSpan.GroupBy(v => v.span.Start))
                {
                    removing.Remove(group.Key);

                    var resolver = new SvelteCodeLensResover(group.Select(v => v.codeLens).ToArray(), provider);
                    //if (exists.TryGetValue(group.Key, out var exist))
                    //{
                    //    exist.Resolver = resolver;
                    //    newResult.Add(exist);
                    //    continue;
                    //}

                    var maxEnd = group.Max(v => v.span.End);
                    var tag = new SvelteCodeLensTag(filePath)
                    {
                        TextSpan = new Span(group.Key, maxEnd - group.Key),
                        Resolver = resolver,
                    };
                    newResult.Add(tag);
                    added.Add(tag);
                }

                result = newResult.ToArray();

                var invalidated = removing.Values.Concat(added);
                foreach (var item in invalidated)
                {
                    var snapshotSpan = new SnapshotSpan(buffer.CurrentSnapshot, item.TextSpan);
                    TagsChanged.Invoke(this, new SnapshotSpanEventArgs(snapshotSpan));
                }

                cancellationTokenSource = null;
            }

            private int OffsetAt(Position position)
            {
                return CodeLensProvider.OffsetAt(buffer, position);
            }

            private Span ConvertSpan(Range range, bool mapToStart)
            {
                var start = OffsetAt(range.Start);
                var end = OffsetAt(range.End);

                if (mapToStart && range.Start.Character != 0)
                {
                    var line = buffer.CurrentSnapshot.GetLineFromPosition(start);
                    start = FirstNonWhitespace(line.GetText()) + line.Start.Position;
                }

                return new Span(start, end - start);
            }

            private int FirstNonWhitespace(string text)
            {
                var chars = text.ToCharArray();
                for (int i = 0; i < chars.Length; i++)
                {
                    var _char = chars[i];

                    if (_char != ' ' && _char != '\t')
                    {
                        return i;
                    }
                }

                return 0;
            }
        }

        private class SvelteCodeLensTag : ICodeLensTag3
        {
            public SvelteCodeLensTag(string filePath)
            {
                Descriptor = new SvelteCodeLensDescriptor
                {
                    FilePath = filePath,
                };
            }

            public ICodeLensDescriptor Descriptor { get; private set; }

            public CodeLensTagProperties Properties => new CodeLensTagProperties(true);

            public ICodeLensDescriptorContextProvider DescriptorContextProvider => Resolver;

            public event EventHandler Disconnected
            {
                add { }
                remove { }
            }

            internal SvelteCodeLensResover Resolver { get; set; }

            internal Span TextSpan { get; set; }
        }

        private class SvelteCodeLensResover : ICodeLensDescriptorContextProvider
        {
            private readonly CodeLensProvider taggerProvider;
            private CodeLens[] LspCodeLens { get; set; }
            private CodeLensDescriptorContext Resolved { get; set; }
            private Span Span { get; set; }

            public SvelteCodeLensResover(
                CodeLens[] lspCodeLens,
                CodeLensProvider taggerProvider)
            {
                LspCodeLens = lspCodeLens;
                this.taggerProvider = taggerProvider;
            }

            public async Task<CodeLensDescriptorContext> GetCurrentContextAsync()
            {
                if (Resolved != null)
                {
                    return Resolved;
                }

                var resolvedCodelens = await Task.WhenAll(LspCodeLens.Select(v => taggerProvider.ResolveAsync(v)));

                LspCodeLens = resolvedCodelens;
                Resolved = new CodeLensDescriptorContext(Span);
                Resolved.Properties.Add("Values", resolvedCodelens.Select(v => new CodeLensInfo
                {
                    Type = ((v.Data as JObject)?.ToObject<CodelensData>())?.Type ?? 
                        (v.Command.Title != null && v.Command.Title.ToLower().Contains("mode") ? "rune" : null),
                    Title = v.Command.Title,
                    References = ExtractReferences(v.Command.Arguments)
                }).ToArray());

                return Resolved;
            }

            private List<CodelensReferenceInfo> ExtractReferences(object[] arguments)
            {
                if (arguments is null || arguments.Length < 3) 
                {
                    return new List<CodelensReferenceInfo>();
                }

                var arg = arguments[2];
                return (arg as JArray).ToObject<Location[]>().Select(v => new CodelensReferenceInfo
                {
                    ColumnNumber = v.Range.Start.Character,
                    LineNumber = v.Range.Start.Line,
                    FilePath = UriToPath(v.Uri)
                }).ToList();
            }
        }

        private class SvelteCodeLensDescriptor : ICodeLensDescriptor
        {
            public string FilePath { get; set; }

            public Guid ProjectGuid => Guid.Empty;

            public string ElementDescription { get; set; }

            public Span? ApplicableSpan { get; set; }

            public CodeElementKinds Kind { get; set; }
        }

        private class CodelensData
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }
        }
    }
}
