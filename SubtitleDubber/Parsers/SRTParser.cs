using System.Collections.Generic;
using SubtitleDubber.Models;
using System.Text.RegularExpressions;
using System.IO;

namespace SubtitleDubber.Parsers
{
    public class SrtParser : ISubtitleParser
    {
        public string FileExtension { get; set; } = ".srt";

        public List<SubtitleItem> Parse(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            var fileContents = File.ReadAllText(filePath);
        string[] newLines = { "\n", "\r\n" };
        string[] itemDelimiters = { newLines[0] + newLines[0], newLines[1] + newLines[1] };

        var textItems = GetTextItems(fileContents, itemDelimiters);
            var items = new List<SubtitleItem>();
            foreach (var textItem in textItems)
            {
                items.Add(new SubtitleItem(textItem));
            }
            return items;
        }

        private static IEnumerable<string> GetTextItems(string fileContents, string[] itemDelimiters)
        {
            fileContents = fileContents?.Trim();

            if (string.IsNullOrEmpty(fileContents))
            {
                return Enumerable.Empty<string>();
            }

            return fileContents.Split(itemDelimiters, StringSplitOptions.None);
        }

}
}