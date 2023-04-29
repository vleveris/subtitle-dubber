using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubtitleDubber.Models;
using SubtitleDubber.Utils;

namespace SubtitleDubber.Services
{
    public class SubtitleService
    {
        public List<SubtitleStreamDescription> GetSubtitleList(string videoFileName)
        {
            return new CommandExecutor().ExecuteSubtitleListCommand(videoFileName);
        }

        public void DownloadSubtitle(string inputVideoFileName, string outputSubtitleFileName, string subtitleFormat, int subtitleTrackId)
        {
            new CommandExecutor().ExecuteDownloadSubtitleCommand(inputVideoFileName, outputSubtitleFileName, subtitleFormat, subtitleTrackId);
        }
    }
}
