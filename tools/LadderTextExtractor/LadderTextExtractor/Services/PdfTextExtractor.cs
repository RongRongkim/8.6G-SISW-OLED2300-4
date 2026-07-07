using System.IO;
using System.Text;
using UglyToad.PdfPig;

namespace LadderTextExtractor.Services
{
    public sealed class PdfTextExtractor
    {
        public (string Text, int PageCount) Extract(string pdfPath)
        {
            using var document = PdfDocument.Open(pdfPath);
            var builder = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                builder.AppendLine(page.Text);
            }

            return (builder.ToString(), document.NumberOfPages);
        }
    }
}
