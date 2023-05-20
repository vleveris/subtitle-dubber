using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleDubber.Models
{
    public class VoiceInfo
    {

        public string Id { get; set; } = null!;
public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Language { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public string Age { get; set; } = null!;
        public IDictionary<string, string> AdditionalInfo { get; set; }

            }
}
