using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SubtitleDubber.Utils.Commands;
using SubtitleDubber.Models;

namespace SubtitleDubber.Utils
{
    internal class CommandExecutor
    {
        private const char CSVDelimiter = ',';

        public string Execute(Command command)
        {
            command.InitializeArguments();
            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = command.Executable;
                p.StartInfo.Arguments = ConvertToArgumentString(command.Arguments);
if (!string.IsNullOrEmpty(command.WorkingDirectory))
                {
                    p.StartInfo.WorkingDirectory = command.WorkingDirectory;
                                    }
                p.StartInfo.RedirectStandardOutput = true;

                p.Start();
                p.WaitForExit();
                var output = p.StandardOutput.ReadToEnd();
                return output;
            }
        }

        private string ConvertToArgumentString(List<String> arguments)
        {
            return string.Join(" ", arguments);
        }

        public void ExecuteSilenceCommand(string inputFileName, string outputFileName, long msAtStart, long msAtEnd)
        {
            var command = new SilenceCommand();
            command.InputFileName = inputFileName;
            command.OutputFileName = outputFileName;
            command.MsAtStart = msAtStart;
            command.MsAtEnd = msAtEnd;
            Execute(command);
        }

        public List<SubtitleStreamDescription> ExecuteSubtitleListCommand(string inputFileName)
        {
            var command = new SubtitleListCommand();
            command.InputFileName = inputFileName;
            var commandOutput = Execute(command);
            var subtitles = commandOutput.Split(Environment.NewLine);
            var descriptionList = new List<SubtitleStreamDescription>();

            foreach (var subtitle in subtitles)
            {
                if (!string.IsNullOrEmpty(subtitle))
                {
                    var description = new SubtitleStreamDescription();
                    var subtitleParts = subtitle.Split(CSVDelimiter);
                    if (subtitleParts.Length > 1)
                    {
                        long parseValue;
                        var parseResult = long.TryParse(subtitleParts[0], out parseValue);
if (parseResult)
                        {
                            description.Id = parseValue;
                            description.LanguageCode = subtitleParts[1];
                    }
                    if (subtitleParts.Length == 3)
                    {
                        description.Title = subtitleParts[2];
                    }
                    descriptionList.Add(description);
                    }
                }
            }
            return descriptionList;
        }

        public void ExecuteDownloadSubtitleCommand(string inputFileName, string outputFileName, string subtitleFormat, int subtitleTrackId)
        {
            var command = new DownloadSubtitleCommand();
            command.InputFileName = inputFileName;
            command.OutputFileName = outputFileName;
            command.SubtitleFormat = subtitleFormat;
            command.SubtitleTrackId = subtitleTrackId;
            Execute(command);
        }

        public void ExecuteConcatFilesCommand(List<string> parameters, string directory)
        {
            var concatCommand = new SOXCommand();
            concatCommand.Arguments = parameters;
            concatCommand.WorkingDirectory = directory;
            Execute(concatCommand);
                    }

public void ExecuteMergeAudioCommand(string inputVideoFileName, string inputAudioFileName, string outputVideoFileName, int dubbingAudioDelay, int originalAudioVolume)
        {
            var command = new MergeAudioCommand();
            command.InputVideoFileName = inputVideoFileName;
            command.InputAudioFileName = inputAudioFileName;
            command.OutputVideoFileName = outputVideoFileName;
            command.DubbingAudioDelay = dubbingAudioDelay;
            command.OriginalAudioVolume = originalAudioVolume;
            Execute(command);
        }
    }
}
