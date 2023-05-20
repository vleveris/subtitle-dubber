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

        private Process InitiateExecution(Command command)
        {
            command.InitializeArguments();
            var process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = command.Executable;
                process.StartInfo.Arguments = ConvertToArgumentString(command.Arguments);
if (!string.IsNullOrEmpty(command.WorkingDirectory))
                {
                    process.StartInfo.WorkingDirectory = command.WorkingDirectory;
                                    }

            return process;
        }

        private string ConvertToArgumentString(List<String> arguments)
        {
            return string.Join(" ", arguments);
        }

        public string Execute(Command command)
        {
            var process = InitiateExecution(command);
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            process.Dispose();

            return output;
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
                        long id;
                        var parsedResult = long.TryParse(subtitleParts[0], out id);
if (parsedResult)
                        {
                            description.Id = id;
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

public Process ExecuteMergeAudioCommand(string inputVideoFileName, string inputAudioFileName, string outputVideoFileName, int dubbingAudioDelay, int originalAudioVolume)
        {
            var command = new MergeAudioCommand();
            command.InputVideoFileName = inputVideoFileName;
            command.InputAudioFileName = inputAudioFileName;
            command.OutputVideoFileName = outputVideoFileName;
            command.DubbingAudioDelay = dubbingAudioDelay;
            command.OriginalAudioVolume = originalAudioVolume;
            var process = InitiateExecution(command);
            process.StartInfo.RedirectStandardError = true;
            return process;
        }

    }
}
