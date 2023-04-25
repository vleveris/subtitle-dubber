using System.Collections.Generic;

namespace SubtitleDubber.Models.Commands
{
    public class FFMPEGCommand : Command
    {
        private const string CommandName = "ffmpeg";
        public FFMPEGCommand() : base(CommandName, new List<string>())
        {
        }

        public new string Executable { get; }
    }
}
