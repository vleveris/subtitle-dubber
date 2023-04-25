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
            var commandOutput = CommandExecutor.ExecuteSubtitleListCommand(videoFileName);
            var subtitles = commandOutput.Split("\r\n");
            var descriptionList = new List<SubtitleStreamDescription>();

            foreach (var subtitle in subtitles)
            {
                if (!string.IsNullOrEmpty(subtitle))
                {
                    var description = new SubtitleStreamDescription();
                    var subtitleParts = subtitle.Split(",");
                    description.Id = long.Parse(subtitleParts[0]);
                    description.LanguageCode = subtitleParts[1];
                    if (subtitleParts.Length == 3)
                    {
                        description.Title = subtitleParts[2];
                    }
                    descriptionList.Add(description);
                }
            }
            return descriptionList;
        }

        public void DownloadSubtitle(string inputVideoFileName, string outputSubtitleFileName, string subtitleFormat, int subtitleTrackId)
        {
            CommandExecutor.ExecuteDownloadSubtitleCommand(inputVideoFileName, outputSubtitleFileName, subtitleFormat, subtitleTrackId);
        }
    }
}
