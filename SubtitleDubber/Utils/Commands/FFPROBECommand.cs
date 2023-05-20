using System.Collections.Generic;

namespace SubtitleDubber.Utils.Commands
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
