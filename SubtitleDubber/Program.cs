using CommandLine;
using System.Text;
using SubtitleDubber.Utils;
using SubtitleDubber.Helpers;
using SubtitleDubber.Services;

namespace SubtitleDubber
{

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Subtitles";
            var input = "";
            var output = "";
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                input = o.Input;
                output = o.Output;
            });
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(output))
            {
                return;
            }

            Parsers.SrtParser parser = new Parsers.SrtParser();
            var subtitles = parser.Parse(input);
            var _dubbingService = new DubbingService();
//            _dubbingService.CreateSubtitleFiles(subtitles, output);
            }
    }
}