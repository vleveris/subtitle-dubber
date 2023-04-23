using System.Collections.Generic;
using SubtitleDubber.Models;
using System.Text.RegularExpressions;
using System.IO;

namespace SubtitleDubber.Parsers
{
    public class SrtParser : ISubtitleParser
    {
        private const string NewLine = "\n";
        private const string ItemDelimiter = NewLine + NewLine;
        public string FileExtension { get; set; } = ".srt";

        public List<SubtitleItem> Parse(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            var fileContents = File.ReadAllText(filePath);
            var textItems = GetTextItems(fileContents);
            var items = new List<SubtitleItem>();
            foreach (var textItem in textItems)
            {
                items.Add(new SubtitleItem(textItem));
            }
            return items;
        }

        private static IEnumerable<string> GetTextItems(string fileContents)
        {
            fileContents = fileContents?.Trim();

            if (string.IsNullOrEmpty(fileContents))
            {
                return Enumerable.Empty<string>();
            }

            return Regex.Split(fileContents, ItemDelimiter);
        }

}
}