using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleDubber.Models
{
    public class VoiceInfo
    {

        public string Id { get; set; }
public string Name { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string Gender { get; set; }
        public string Age { get; set; }
        public IDictionary<string, string> AdditionalInfo { get; set; }

            }
}
