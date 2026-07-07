using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LadderTextExtractor.Models;

namespace LadderTextExtractor.Services
{
    public sealed class LadderTextParser
    {
        private static readonly Regex RegisterRegex = new Regex(
            @"\b(M[BLWGDOCW]?[0-9A-F]{3,6}|I[LWB][0-9A-F]{3,6}|O[LWB][0-9A-F]{3,6}|I[BW][0-9A-F]{3,6}|D[LWB][0-9A-F]{3,6}|C[WGD][0-9A-F]{3,6})\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CrossRefRegex = new Regex(
            @"\b([HL]\d{2}\.\d{2})[/\\][0-9A-Za-z]+\b",
            RegexOptions.Compiled);

        private static readonly Regex ProgramIdRegex = new Regex(
            @"\b([HL]\d{2}\.\d{2})\b",
            RegexOptions.Compiled);

        public LadderDocument ParseFile(string pdfPath, string text, int pageCount)
        {
            var fileName = Path.GetFileNameWithoutExtension(pdfPath);
            var lines = text
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .ToList();

            var registers = RegisterRegex.Matches(text)
                .Cast<Match>()
                .Select(x => x.Value.ToUpperInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var expressions = lines
                .Where(x => x.StartsWith("EXPRESSION", StringComparison.OrdinalIgnoreCase)
                            || (x.Contains("=") && (x.Contains("IL") || x.Contains("DL") || x.Contains("ML") || x.Contains("OW"))))
                .Distinct()
                .ToList();

            var crossRefs = CrossRefRegex.Matches(text)
                .Cast<Match>()
                .Select(x => x.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var programId = ProgramIdRegex.Match(fileName).Groups[1].Value;
            if (string.IsNullOrWhiteSpace(programId))
            {
                programId = fileName;
            }

            var scanType = "Unknown";
            if (pdfPath.IndexOf("HightScan", StringComparison.OrdinalIgnoreCase) >= 0
                || pdfPath.IndexOf("HighScan", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                scanType = "HighScan";
            }
            else if (pdfPath.IndexOf("LowScan", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                scanType = "LowScan";
            }

            return new LadderDocument
            {
                SourcePath = pdfPath,
                ProgramId = programId,
                ScanType = scanType,
                PageCount = pageCount,
                TextLength = text.Length,
                IsEmpty = text.Trim().Length < 100,
                RawText = text,
                Lines = lines,
                Registers = registers,
                Expressions = expressions,
                CrossReferences = crossRefs
            };
        }
    }
}
