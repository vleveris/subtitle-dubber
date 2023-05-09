using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubtitleDubber.Models;
using SubtitleDubber.Utils;
using SubtitleDubber.Parsers;
using SubtitleDubber.Validators;
using SubtitleDubber.Exceptions;

namespace SubtitleDubber.Services
{
    public class SubtitleService
    {
        private readonly List<ISubtitleParser> _supportedParsers =
            new List<ISubtitleParser>();

        public SubtitleService()
        {
            FillParsers();

        }

        public List<SubtitleStreamDescription> GetSubtitleList(string videoFileName)
        {
            return new CommandExecutor().ExecuteSubtitleListCommand(videoFileName);
        }

        public void DownloadSubtitle(string inputVideoFileName, string outputSubtitleFileName, string subtitleFormat, int subtitleTrackId)
        {
            new CommandExecutor().ExecuteDownloadSubtitleCommand(inputVideoFileName, outputSubtitleFileName, subtitleFormat, subtitleTrackId);
        }

        private void FillParsers()
        {
            _supportedParsers.Add(new SSAParser());
            _supportedParsers.Add(new VTTParser());
            _supportedParsers.Add(new SrtParser());
        }

        public List<SubtitleItem> GetSubtitlesFromFile(string fileName)
        {
            var selectedParser = _supportedParsers.Where(parser => parser.FileExtension == Path.GetExtension(fileName))
    .Select(parser => parser).FirstOrDefault();

            var subtitles = selectedParser.Parse(fileName);
            var validator = new SubtitlesValidator();
            if (!validator.Validate(subtitles))
            {
                throw new SubtitleException("Subtitle file contains invalid subtitle items.");
            }
            return subtitles;
        }

        }
}