using CommandLine;

namespace SubtitleDubber
{
    public class Options
    {
        [Option('i', "input", Required = true,
            HelpText = "Set the subtitle file in SubRip format.")]
        public string Input { get; set; }

        [Option('o', "output", Required = true, HelpText = "Set the path to save audio track.")]
        public string Output { get; set; }

    }
}