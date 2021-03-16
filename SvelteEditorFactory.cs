using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Package;

namespace SvelteVisualStudio
{
    [Guid("91B34873-62FF-42E3-9664-A518B922478F")]
    public class SvelteEditorFactory : EditorFactory
    {
        public SvelteEditorFactory(SvelteVSPackage package)
            : base(package)
        { }
    }
}
