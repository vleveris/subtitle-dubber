using System.Collections.Generic;

namespace SubtitleDubber.Utils.Commands
{
    public class SubtitleListCommand : FFPROBECommand
    {
        private const string RawCommand = "-loglevel error -select_streams s -show_entries stream=index:stream_tags=language:stream_tags=title -of csv=p=0";
        public string InputFileName { get; set; } = null!;

        public override void InitializeArguments()
        {
            if (string.IsNullOrEmpty(InputFileName))
            {
                throw new ArgumentNullException("InputFileName");
            }
            Arguments.Add(RawCommand);
            Arguments.Add(EncloseFileNameWithQuotes(InputFileName));
        }

    }
}
