﻿using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Shell;
using SvelteVisualStudio.Attributes;
using System;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace SvelteVisualStudio
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    ///
    /// We need this in order for to use the ProvideEditorExtension attribute to specify a priority
    /// higher than the XML editor.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideEditorExtension(typeof(SvelteEditorFactory), SvelteContentDefinition.Extension, 100)]
    [ProvideWorkspaceSettings("VSWorkspaceSettings", "SvelteLanguageSettings.json")]
    [ProvideConfig(
        CommonEditorConstants.TextMateRepositoryKey,
        SvelteContentDefinition.Identifier, "$PackageFolder$\\Grammars")]
    [ProvideConfig("TextMate\\LanguageConfiguration\\GrammarMapping",
        "source.svelte", languageConfigPath)]
    [ProvideConfig("TextMate\\LanguageConfiguration\\ContentTypeMapping",
        SvelteContentDefinition.Identifier, languageConfigPath)]
    [ProvideEditorLogicalView(typeof(SvelteEditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    public class SvelteVSPackage : AsyncPackage
    {
        private const string languageConfigPath = "$PackageFolder$\\LanguageConfig\\language-configuration.json";

        /// <summary>
        /// Initializes a new instance of the <see cref="SvelteVSPackage"/> class.
        /// </summary>
        public SvelteVSPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            RegisterEditorFactory(new SvelteEditorFactory(this));
        }

        #endregion
    }
}
