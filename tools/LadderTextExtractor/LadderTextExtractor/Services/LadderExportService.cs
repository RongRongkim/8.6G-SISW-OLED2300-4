using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using LadderTextExtractor.Models;

namespace LadderTextExtractor.Services
{
    public sealed class LadderExportService
    {
        private readonly PdfTextExtractor _pdfTextExtractor = new PdfTextExtractor();
        private readonly LadderTextParser _ladderTextParser = new LadderTextParser();
        private readonly GlobalValueLoader _globalValueLoader = new GlobalValueLoader();

        public ExtractionSummary Export(string sourceRoot, string outputRoot, string globalValuePath)
        {
            Directory.CreateDirectory(outputRoot);

            var registerComments = _globalValueLoader.Load(globalValuePath);
            var pdfFiles = FindLadderPdfs(sourceRoot).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
            var summary = new ExtractionSummary
            {
                SourceRoot = sourceRoot,
                OutputRoot = outputRoot,
                TotalFiles = pdfFiles.Count
            };

            foreach (var pdfPath in pdfFiles)
            {
                try
                {
                    var fileInfo = new FileInfo(pdfPath);
                    if (fileInfo.Length < 100)
                    {
                        summary.EmptyFiles++;
                        summary.Documents.Add(new LadderDocument
                        {
                            SourcePath = pdfPath,
                            IsEmpty = true
                        });
                        continue;
                    }

                    var (text, pageCount) = _pdfTextExtractor.Extract(pdfPath);
                    var doc = _ladderTextParser.ParseFile(pdfPath, text, pageCount);
                    if (doc.IsEmpty)
                    {
                        summary.EmptyFiles++;
                    }
                    else
                    {
                        summary.SuccessFiles++;
                    }

                    WriteTextFile(doc, outputRoot);
                    WriteJsonFile(doc, registerComments, outputRoot);
                    summary.Documents.Add(doc);
                }
                catch
                {
                    summary.FailedFiles++;
                }
            }

            WriteSummary(summary, outputRoot);
            WriteRegisterIndex(summary, registerComments, outputRoot);
            return summary;
        }

        private static IEnumerable<string> FindLadderPdfs(string sourceRoot)
        {
            if (!Directory.Exists(sourceRoot))
            {
                return Enumerable.Empty<string>();
            }

            return Directory.EnumerateFiles(sourceRoot, "*.pdf", SearchOption.AllDirectories)
                .Where(x => x.IndexOf("Ladder_", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static void WriteTextFile(LadderDocument doc, string outputRoot)
        {
            var relative = BuildRelativePath(doc.SourcePath);
            var target = Path.Combine(outputRoot, "text", relative + ".txt");
            Directory.CreateDirectory(Path.GetDirectoryName(target) ?? outputRoot);
            File.WriteAllText(target, doc.RawText, Encoding.UTF8);
        }

        private static void WriteJsonFile(LadderDocument doc, Dictionary<string, string> registerComments, string outputRoot)
        {
            var relative = BuildRelativePath(doc.SourcePath);
            var target = Path.Combine(outputRoot, "json", relative + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(target) ?? outputRoot);

            var payload = new
            {
                doc.SourcePath,
                doc.ProgramId,
                doc.ScanType,
                doc.PageCount,
                doc.TextLength,
                doc.IsEmpty,
                doc.Registers,
                doc.Expressions,
                doc.CrossReferences,
                RegisterComments = doc.Registers.ToDictionary(
                    x => x,
                    x => registerComments.TryGetValue(x, out var comment) ? comment : string.Empty,
                    StringComparer.OrdinalIgnoreCase)
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(target, json, Encoding.UTF8);
        }

        private static void WriteSummary(ExtractionSummary summary, string outputRoot)
        {
            var target = Path.Combine(outputRoot, "index.json");
            var payload = new
            {
                summary.SourceRoot,
                summary.OutputRoot,
                summary.TotalFiles,
                summary.SuccessFiles,
                summary.EmptyFiles,
                summary.FailedFiles,
                Documents = summary.Documents.Select(x => new
                {
                    x.SourcePath,
                    x.ProgramId,
                    x.ScanType,
                    x.PageCount,
                    x.TextLength,
                    x.IsEmpty,
                    RegisterCount = x.Registers?.Count ?? 0,
                    ExpressionCount = x.Expressions?.Count ?? 0,
                    CrossReferenceCount = x.CrossReferences?.Count ?? 0
                })
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(target, json, Encoding.UTF8);
        }

        private static void WriteRegisterIndex(ExtractionSummary summary, Dictionary<string, string> registerComments, string outputRoot)
        {
            var index = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var doc in summary.Documents.Where(x => !x.IsEmpty))
            {
                foreach (var register in doc.Registers)
                {
                    if (!index.TryGetValue(register, out var programs))
                    {
                        programs = new List<string>();
                        index[register] = programs;
                    }

                    if (!programs.Contains(doc.ProgramId, StringComparer.OrdinalIgnoreCase))
                    {
                        programs.Add(doc.ProgramId);
                    }
                }
            }

            var lines = index
                .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .Select(x =>
                {
                    registerComments.TryGetValue(x.Key, out var comment);
                    return x.Key + "\t" + comment + "\t" + string.Join(",", x.Value);
                });

            var target = Path.Combine(outputRoot, "register-index.tsv");
            File.WriteAllLines(target, lines, Encoding.UTF8);
        }

        private static string BuildRelativePath(string pdfPath)
        {
            var name = Path.GetFileNameWithoutExtension(pdfPath) ?? "unknown";
            var parent = Path.GetFileName(Path.GetDirectoryName(pdfPath) ?? string.Empty);
            if (string.IsNullOrWhiteSpace(parent))
            {
                return SanitizeFileName(name);
            }

            return SanitizeFileName(parent) + "__" + SanitizeFileName(name);
        }

        private static string SanitizeFileName(string value)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(c, '_');
            }

            return value.Replace(' ', '_');
        }
    }
}
