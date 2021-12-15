using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Settings;
using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
using StreamJsonRpc;
using SvelteVisualStudio.MiddleLayers;
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
    // The ILanguageClientCustomMessage used in the official LSP docs example doesn't work anymore
    class SvelteLanguageClientBase : ILanguageClientCustomMessage2 
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

        // Should use gitignore pattern here, not all glob works
        public IEnumerable<string> FilesToWatch => new[] {"*.ts" , "*.js"};

        public object MiddleLayer { get; }
        protected readonly MiddleLayerHost middleLayerHost;

        public object CustomMessageTarget => new { };

        public event AsyncEventHandler<EventArgs> StartAsync;

        public SvelteLanguageClientBase([Import] IVsFolderWorkspaceService workspaceService)
        {
            this.workspaceService = workspaceService;
            var middleLayer = new MiddleLayerHost();

            MiddleLayer = middleLayerHost = middleLayer;
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
            return Task.CompletedTask;
        }
    }
}
