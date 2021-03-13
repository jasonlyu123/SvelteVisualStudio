using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
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

        public IEnumerable<string> ConfigurationSections => null;

        public object InitializationOptions => null;

        public IEnumerable<string> FilesToWatch => new[] {
            "**/*.{js,ts}"
        };

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            await Task.Yield();

            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var lsPath = Path.Combine(directory, "node_modules", "svelte-language-server", "bin", "server.js");
            var port = 6009;
            var args = string.Join(
                " ",
                $"\"{lsPath}\"",
                "--stdio",
                $"--clientProcessId={Process.GetCurrentProcess().Id}",
                $"--inspect=${port}");

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
