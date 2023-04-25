using System.Collections.Generic;

namespace SubtitleDubber.Utils.Commands
{
    public class SilenceCommand : SOXCommand
    {
        private const string PadArgument = "pad";
        public string InputFileName { get; set; } = null!;
        public string OutputFileName { get; set; } = null!;
        public long MsAtStart { get; set; } = 0;
        public long MsAtEnd { get; set; } = 0;

        public override void InitializeArguments()
        {
            if (string.IsNullOrEmpty(InputFileName))
            {
                throw new ArgumentNullException("InputFileName");
            }
            if (string.IsNullOrEmpty(OutputFileName))
            {
                throw new ArgumentNullException("OutputFileName");
            }
            Arguments.Add(InputFileName);
            Arguments.Add(OutputFileName);
            Arguments.Add(PadArgument);
            Arguments.Add(FormatSilenceTime(MsAtStart));
            Arguments.Add(FormatSilenceTime(MsAtEnd));
        }

        private string FormatSilenceTime(long ms)
        {
            var timeString = string.Empty + ms / 1000;
            long msRemainder = ms % 1000;
            if (msRemainder != 0)
            {
                timeString += ".";
                if (msRemainder < 10)
                {
                    timeString += "00";
                }
                else if (msRemainder < 100)
                {
                    timeString += "0";
                }
                timeString = timeString + msRemainder;
            }
            return timeString;
        }

    }
}
