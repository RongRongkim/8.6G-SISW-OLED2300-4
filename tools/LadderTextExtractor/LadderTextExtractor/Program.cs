using System;
using System.IO;
using LadderTextExtractor.Services;

namespace LadderTextExtractor
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var sourceRoot = args.Length > 0 ? args[0] : FindDefaultSourceRoot();
            var outputRoot = args.Length > 1 ? args[1] : Path.Combine(sourceRoot, "output", "ladder-text");
            var globalValuePath = args.Length > 2
                ? args[2]
                : Path.Combine(sourceRoot, "GlobalValue.txt");

            if (!Directory.Exists(sourceRoot))
            {
                Console.WriteLine("Source root not found: " + sourceRoot);
                Console.WriteLine("Usage: LadderTextExtractor [sourceRoot] [outputRoot] [globalValuePath]");
                return 1;
            }

            Console.WriteLine("Source : " + sourceRoot);
            Console.WriteLine("Output : " + outputRoot);
            Console.WriteLine("Global : " + globalValuePath);

            var service = new LadderExportService();
            var summary = service.Export(sourceRoot, outputRoot, globalValuePath);

            Console.WriteLine("Total  : " + summary.TotalFiles);
            Console.WriteLine("OK     : " + summary.SuccessFiles);
            Console.WriteLine("Empty  : " + summary.EmptyFiles);
            Console.WriteLine("Failed : " + summary.FailedFiles);
            Console.WriteLine("Index  : " + Path.Combine(outputRoot, "index.json"));
            return summary.FailedFiles > 0 ? 2 : 0;
        }

        private static string FindDefaultSourceRoot()
        {
            var current = AppContext.BaseDirectory;
            for (var i = 0; i < 8; i++)
            {
                var candidate = Path.GetFullPath(Path.Combine(current, "..", "..", "..", "..", ".."));
                if (Directory.Exists(Path.Combine(candidate, "Ladder_HightScan"))
                    || Directory.Exists(Path.Combine(candidate, "Ladder_LowScan_260608_R01")))
                {
                    return candidate;
                }

                current = candidate;
            }

            return Directory.GetCurrentDirectory();
        }
    }
}
