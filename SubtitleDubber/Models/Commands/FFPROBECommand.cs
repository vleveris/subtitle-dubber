using System.Collections.Generic;

namespace SubtitleDubber.Models.Commands
{
    public class FFPROBECommand : Command
    {
        private const string CommandName = "ffprobe";
        public FFPROBECommand() : base(CommandName, new List<string>())
        {
        }

        public new string Executable { get; }
    }
}
