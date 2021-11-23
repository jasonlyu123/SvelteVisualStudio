using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace SvelteVisualStudio_2022
{
    [Guid(PackageGuidString)]
    public sealed class SvelteVSPackage : SvelteVisualStudio.SvelteVSPackage
    {
        /// <summary>
        /// SvelteVSPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "713b6de6-e01d-4769-8e2b-fede43b24bec";
    }
}
