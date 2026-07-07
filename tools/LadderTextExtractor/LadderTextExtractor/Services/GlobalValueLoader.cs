using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LadderTextExtractor.Services
{
    public sealed class GlobalValueLoader
    {
        public Dictionary<string, string> Load(string path)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(path))
            {
                return map;
            }

            var bytes = File.ReadAllBytes(path);
            string text;
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                text = Encoding.Unicode.GetString(bytes);
            }
            else
            {
                text = Encoding.UTF8.GetString(bytes);
            }

            foreach (var rawLine in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var line = rawLine.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                var parts = line.Split('\t');
                if (parts.Length < 2)
                {
                    continue;
                }

                var name = parts[0].Trim().Trim('"');
                var comment = parts[1].Trim().Trim('"');
                if (name.Length > 0 && !map.ContainsKey(name))
                {
                    map[name] = comment;
                }
            }

            return map;
        }
    }
}
