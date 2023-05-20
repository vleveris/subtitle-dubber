using System.Collections.Generic;

namespace SubtitleDubber.Utils.Commands
{
    public class SOXCommand : Command
    {
        private const string CommandName = "sox";

        public SOXCommand() : base(CommandName, new List<string>())
        {
        }

        public new string Executable { get; }
    }
}
