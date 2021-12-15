using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Settings;
using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
using StreamJsonRpc;
using SvelteVisualStudio.MiddleLayers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SvelteVisualStudio
{
    // The ILanguageClientCustomMessage used in the official LSP docs example doesn't work anymore
    class SvelteLanguageClientBase : ILanguageClientCustomMessage2
    {
        public string Name => "Svelte For Visual Studio";
        private const string configScope = "svelte";
        private readonly IVsFolderWorkspaceService workspaceService;
        private JsonRpc rpc;

        public IEnumerable<string> ConfigurationSections => new[]
        {
            configScope,
            "typescript",
            "javascript"
        };

        public object InitializationOptions => null;

        // Should use gitignore pattern here, not all glob works
        public IEnumerable<string> FilesToWatch => new[] { "*.ts", "*.js" };

        public object MiddleLayer { get; }
        protected readonly MiddleLayerHost middleLayerHost;

        public object CustomMessageTarget => new { };

        public event AsyncEventHandler<EventArgs> StartAsync;

        public SvelteLanguageClientBase(
            IVsFolderWorkspaceService workspaceService,
            TsJsTextBufferManager tsJsTextBufferManager)
        {
            this.workspaceService = workspaceService;
            var middleLayer = new MiddleLayerHost();

            MiddleLayer = middleLayerHost = middleLayer;

            TrackTsJsUpdate(tsJsTextBufferManager);
        }


        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            await Task.Yield();
            var workspace = workspaceService.CurrentWorkspace;

            // if user open a solution or project, workspace would be null
            var settingsManager = workspace?.GetSettingsManager();
            var settings = settingsManager?.GetAggregatedSettings(SettingsTypes.Generic);
            var args = GetLanguageServerArguments(settings);

            var info = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = args,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process
            {
                StartInfo = info
            };

            if (process.Start())
            {
                return new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
            }

            return null;
        }

        private static string GetLanguageServerArguments(IWorkspaceSettings settings)
        {
            var portSettings = settings?.Property<int?>("svelte.language-server.port");

            // For security reason don't allow this setting on release
            string lsPathSettings =
#if DEBUG
                settings?.Property<string>("svelte.language-server.ls-path");
#else
                null;
#endif

            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // npm install in build step, defined in the csproj
            var lsPath = string.IsNullOrEmpty(lsPathSettings) ?
                Path.Combine(directory, "node_modules", "svelte-language-server", "bin", "server.js") :
                lsPathSettings;

            var port = portSettings > 0 ? portSettings : 6009;
            var args = string.Join(
                " ",
                $"--inspect={port}",
                $"\"{lsPath}\"",
                "--stdio",
                $"--clientProcessId={Process.GetCurrentProcess().Id}");
            return args;
        }

        public async Task OnLoadedAsync()
        {
            await StartAsync.InvokeAsync(this, EventArgs.Empty);
        }

        public Task OnServerInitializeFailedAsync(Exception e)
        {
            return Task.CompletedTask;
        }

        public Task OnServerInitializedAsync()
        {
            return Task.CompletedTask;
        }

        public Task AttachForCustomMessageAsync(JsonRpc rpc)
        {
            this.rpc = rpc;
            return Task.CompletedTask;
        }

        private void TrackTsJsUpdate(TsJsTextBufferManager tsJsTextBufferManager)
        {
            // buffers opened before client initialized
            foreach (var item in tsJsTextBufferManager.Buffers)
            {
                TsJsTextBufferManager_BufferRegistered(item);
            }

            tsJsTextBufferManager.BufferRegistered += TsJsTextBufferManager_BufferRegistered;
            tsJsTextBufferManager.BufferUnRegistered += TsJsTextBufferManager_BufferUnRegistered;
        }

        private void TsJsTextBufferManager_BufferRegistered(ITextBuffer buffer)
        {
            buffer.ChangedLowPriority += Buffer_ChangedLowPriority;
        }

        private void TsJsTextBufferManager_BufferUnRegistered(ITextBuffer buffer)
        {
            buffer.ChangedLowPriority -= Buffer_ChangedLowPriority;
        }

        private void Buffer_ChangedLowPriority(object sender, TextContentChangedEventArgs e)
        {
            if (
                rpc is null ||
                !(sender is ITextBuffer buffer) ||
                !buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument textDocument) ||
                textDocument is null)
            {
                return;
            }

            var before = e.Before;

            var changes = e.Changes.Select(change => ConvertChange(before, change)).ToList();
            var arg = new { uri = new Uri(textDocument.FilePath).AbsoluteUri, changes };

            _ = rpc.NotifyAsync("$/onDidChangeTsOrJsFile", arg);
        }

        private static TextDocumentContentChangeEvent ConvertChange(
            ITextSnapshot before,
            ITextChange change)
        {
            var line = before.GetLineFromPosition(change.OldPosition);
            var character = change.OldPosition - line.Start.Position;
            var length = change.OldSpan.Length;

            return new TextDocumentContentChangeEvent
            {
                Range = new Range
                {
                    Start = new Position(line.LineNumber, character),
                    End = new Position(line.LineNumber, character + length)
                },
                Text = change.NewText
            };
        }
    }
}
