using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Settings;
using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SvelteVisualStudio
{
    [ContentType(SvelteContentDefinition.Identifier)]
    [Export(typeof(ILanguageClient))]
    class SvelteLanguageClient : ILanguageClient
    {
        public string Name => "Svelte For Visual Studio";
        private const string configScope = "svelte";
        private readonly IVsFolderWorkspaceService workspaceService;

        public IEnumerable<string> ConfigurationSections => new[]
        {
            configScope,
            "typescript",
            "javascript"
        };

        public object InitializationOptions => null;

        public IEnumerable<string> FilesToWatch => new[] {
            "**/*.{js,ts}"
        };

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        [ImportingConstructor]
        public SvelteLanguageClient([Import] IVsFolderWorkspaceService workspaceService)
        {
            this.workspaceService = workspaceService;
        }

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            await Task.Yield();
            var workspace = workspaceService.CurrentWorkspace;
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
            var lsPathSettings = settings?.Property<string>("svelte.language-server.ls-path");

            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var lsPath = string.IsNullOrEmpty(lsPathSettings) ?
                Path.Combine(directory, "node_modules", "svelte-language-server", "bin", "server.js") :
                lsPathSettings;
            var port = portSettings > 0 ? portSettings : 6009;
            var args = string.Join(
                " ",
                $"\"{lsPath}\"",
                "--stdio",
                $"--clientProcessId={Process.GetCurrentProcess().Id}",
                $"--inspect=${port}");
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
    }
}
