using System.Collections.Generic;

namespace SubtitleDubber.Utils.Commands
{
    public class MergeAudioCommand : FFMPEGCommand
    {
        private const string InputOption = "-i";
        private const string OverrideOption = "-y";
        private const string CopyVideoCodecOption = "-c:v copy";
        private const string ComplexFiltergraph = "-filter_complex \"[1:a] adelay=<delay>|<delay> [dubbingAudio]; [0:a] volume=<volume> [originalAudio]; [originalAudio] [dubbingAudio] amix=inputs=2:duration=longest [audioOut]\"";
        private const string IncludeStreamsOption = "-map 0 -map \"[audioOut]\"";
        private const int MinDubbingAudioDelay = 0;
        private const int MinOriginalAudioVolume = 0;
        private const int MaxOriginalAudioVolume = 100;

        public string InputVideoFileName { get; set; } = null!;
        public string InputAudioFileName { get; set; } = null!;
                public string OutputVideoFileName { get; set; } = null!;

public int DubbingAudioDelay { get; set; }
        public int OriginalAudioVolume { get; set; }

        public override void InitializeArguments()
        {
            if (string.IsNullOrEmpty(InputVideoFileName))
            {
                throw new ArgumentNullException("InputVideoFileName");
            }

            if (string.IsNullOrEmpty(InputAudioFileName))
            {
                throw new ArgumentNullException("InputAudioFileName");
            }

            if (string.IsNullOrEmpty(OutputVideoFileName))
            {
                throw new ArgumentNullException("OutputVideoFileName");
            }

if (DubbingAudioDelay < MinDubbingAudioDelay)
            {
                throw new ArgumentOutOfRangeException("Dubbing audio delay value must be positive number or 0.");
                            }

if (OriginalAudioVolume < MinOriginalAudioVolume || OriginalAudioVolume > MaxOriginalAudioVolume)
            {
                throw new ArgumentOutOfRangeException("Original audio volume must be between " + MinOriginalAudioVolume + " and " + MaxOriginalAudioVolume + ".");
            }

            Arguments.Add(InputOption);
            Arguments.Add(InputVideoFileName);
            Arguments.Add(InputOption);
            Arguments.Add(InputAudioFileName);
            Arguments.Add(CopyVideoCodecOption);
                        Arguments.Add(ComplexFiltergraph.Replace("<delay>", DubbingAudioDelay.ToString()).Replace("<volume>", ConvertToVolumeString(OriginalAudioVolume)));
            Arguments.Add(IncludeStreamsOption);
            Arguments.Add(OverrideOption);
            Arguments.Add(OutputVideoFileName);
        }

        private string ConvertToVolumeString(int volume)
        {
            if (volume == MaxOriginalAudioVolume)
            {
                return "1";
            }
            return "0." + volume / 10;
        }
    }
}
