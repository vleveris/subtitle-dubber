using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleDubber.Models
{
    public class SubtitleStreamDescription
    {

public long Id { set; get; }
        public string LanguageCode { set; get; } = null!;
        public string Title { set; get; } = null!;
        public override string ToString()
        {
            var stringOutput = LanguageCode;
            if (!string.IsNullOrEmpty(Title))
            {
                stringOutput = stringOutput + " (" + Title + ")";
            }
            return stringOutput;
        }
    }
}
