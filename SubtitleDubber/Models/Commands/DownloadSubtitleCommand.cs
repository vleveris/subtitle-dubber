using System.Collections.Generic;

namespace SubtitleDubber.Models.Commands
{
    public class DownloadSubtitleCommand : FFMPEGCommand
    {
        private const string InputOption = "-i";
        private const string MapExpression = "-map 0:";
        private const string SubtitleStreamOption = "-c:s";
        private const string OverrideOption = "-y";
        private readonly string[] AvailableSubtitleFormats = { "srt" };
        public string InputFileName { get; set; } = null!;
        public string OutputFileName { get; set; } = null!;
        public int SubtitleTrackId { get; set; }
        public string SubtitleFormat { get; set; } = null!;

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

            if (SubtitleTrackId<0)
            {
                throw new ArgumentOutOfRangeException("SubtitleTrackId<0");
            }

            if (string.IsNullOrEmpty(SubtitleFormat))
            {
                throw new ArgumentNullException("SubtitleFormat");
            }

            if (!AvailableSubtitleFormats.Contains(SubtitleFormat))
            {
                throw new ArgumentOutOfRangeException("Unsupported subtitle format.");
                // todo: FFMPEG supported subtitle formats should go into AvailableSubtitleFormats array
// Separate exception class might be needed
                            }

            Arguments.Add(InputOption);
            Arguments.Add(InputFileName);
            Arguments.Add(MapExpression + SubtitleTrackId.ToString());
            Arguments.Add(SubtitleStreamOption);
            Arguments.Add(SubtitleFormat);
            Arguments.Add(OutputFileName);
            Arguments.Add(OverrideOption);
        }

    }
}
