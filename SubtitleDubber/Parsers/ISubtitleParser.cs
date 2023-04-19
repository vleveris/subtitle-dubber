using System.Collections.Generic;
using SubtitleDubber.Models;

namespace SubtitleDubber.Parsers
{
    public interface ISubtitleParser
    {
        string FileExtension { get; set; }

        List<SubtitleItem> Parse(string filePath);
    }
}