using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleDubber.Models
{
    public class FileFormat
    {
public string Extension { get; set; }
        public string Title { get; set; }

        public static string DefaultSubtitleFileExtension = "srt";

        public FileFormat(string extension, string title)
        {
            Extension = extension.ToLower();
            Title = title;
        }

        public override string ToString()
        {
            return Title + " (" + Extension.ToUpper() + ")";
        }
    }
}
