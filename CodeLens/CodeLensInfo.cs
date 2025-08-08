using System.Collections.Generic;

namespace SvelteVisualStudio.CodeLens
{
    internal class CodeLensInfo
    {
        public string Type { get; set; }
        public string Title { get; set; }

        public List<CodelensReferenceInfo> References { get; set; }
    }

    internal class CodelensReferenceInfo 
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
    }

    internal class CodelensReferenceInfoFull
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public string ReferenceLineText { get; set; }
        public int ReferenceStart { get; set; }
        public int ReferenceLength { get; set; }
        public string BeforeReferenceText1 { get; set; }
        public string BeforeReferenceText2 { get; set; }
        public string AfterReferenceText1 { get; set; }
        public string AfterReferenceText2 { get; set; }
    }
}
