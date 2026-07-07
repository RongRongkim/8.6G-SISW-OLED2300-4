using System.Collections.Generic;

namespace LadderTextExtractor.Models
{
    public sealed class LadderDocument
    {
        public string SourcePath { get; set; } = "";
        public string ProgramId { get; set; } = "";
        public string ScanType { get; set; } = "";
        public int PageCount { get; set; }
        public int TextLength { get; set; }
        public bool IsEmpty { get; set; }
        public string RawText { get; set; } = "";
        public List<string> Lines { get; set; } = new List<string>();
        public List<string> Registers { get; set; } = new List<string>();
        public List<string> Expressions { get; set; } = new List<string>();
        public List<string> CrossReferences { get; set; } = new List<string>();
    }

    public sealed class ExtractionSummary
    {
        public string SourceRoot { get; set; } = "";
        public string OutputRoot { get; set; } = "";
        public int TotalFiles { get; set; }
        public int SuccessFiles { get; set; }
        public int EmptyFiles { get; set; }
        public int FailedFiles { get; set; }
        public List<LadderDocument> Documents { get; set; } = new List<LadderDocument>();
    }
}
