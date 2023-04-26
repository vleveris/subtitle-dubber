using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubtitleDubber.Models;
using SubtitleDubber.Helpers;
using SubtitleDubber.Utils;
using System.Speech.Synthesis;
using SubtitleDubber.Parsers;
using System.Collections;

namespace SubtitleDubber.Services
{
    public class DubbingService
    {
        private SpeechService _speechService = new();
        private SubtitleService _subtitleService = new();
        private const string TemporarySubtitleFileName = "subtitle.srt";
        private const int MaxFilesForOneCommand = 500;
        private List<string> tempFiles = new List<string>();

        public void Dub(int subtitleTrackId, string inputVideoFileName, string outputVideoFileName, IProgress<int> progress, bool useSox = true)
        {
            //        var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var tempDirectory = "C:\\hardas\\SubtitleDubber";
            FileUtils.CreateDirectory(tempDirectory);
            var inputSubtitleFileName = tempDirectory + Path.DirectorySeparatorChar + TemporarySubtitleFileName;
            _subtitleService.DownloadSubtitle(inputVideoFileName, inputSubtitleFileName, FileFormat.DefaultSubtitleFileExtension, subtitleTrackId);
            tempFiles.Add(inputSubtitleFileName);
            Dub(inputSubtitleFileName, outputVideoFileName, progress, tempDirectory, useSox);
        }

        public void Dub(string inputSubtitleFileName, string outputVideoFileName, IProgress<int> progress, string tempDirectory = "", bool useSox = true)
        {
if (string.IsNullOrEmpty(tempDirectory))
            {
                //        var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                tempDirectory = "C:\\hardas\\SubtitleDubber";
                Directory.CreateDirectory(tempDirectory);
            }
            var parser = new SrtParser();
            var subtitles = parser.Parse(inputSubtitleFileName);

            string subtitleText;
            long fileDuration, msAtStart = 0, subtitleSpeechDuration, silenceDuration;
            var builder = new PromptBuilder();
            var silences = new long[subtitles.Count + 1];
            foreach (var subtitle in subtitles)
            {
                subtitleText = subtitle.Text.RemoveAllFormatting();
                var fileName = tempDirectory + Path.DirectorySeparatorChar + subtitle.Index;
                if (subtitle.Index == subtitles.Count)
                {
                    subtitleSpeechDuration = (long)subtitle.Duration.TotalActiveTime.TotalMilliseconds;
                }
                else
                {
                    subtitleSpeechDuration = (long)subtitles[subtitle.Index].Duration.Start.Subtract(subtitle.Duration.Start).TotalMilliseconds;
                }
                var overlap = SpeakSubtitle(subtitleText, fileName, subtitleSpeechDuration);
                tempFiles.Add(fileName);
                if (overlap > 0)
                {
                    var silenceCutValue = CalculateSpeechTime(silences[subtitle.Index - 1], overlap);
                    if (silenceCutValue == 0)
                    {

                    }
                    else
                    {
                        subtitleSpeechDuration += silenceCutValue;
                        silences[subtitle.Index - 1] -= silenceCutValue;
                    }
                }
                if (subtitle.Index == 1)
                {
                    msAtStart = (long)subtitle.Duration.Start.TotalMilliseconds;
                    if (!useSox)
                    {
                        builder.AppendBreak(new TimeSpan(0, 0, 0, (int)msAtStart / 1000, (int)msAtStart % 1000));
                    }
                    else
                    {
                        silences[0] = msAtStart;
                    }
                }
                fileDuration = FileUtils.GetAudioFileDuration(fileName);
                silenceDuration = subtitleSpeechDuration - fileDuration;
                if (useSox)
                {
                    silences[subtitle.Index] = silenceDuration;
                }
                else
                {
                    builder.AppendAudio(fileName);
                    var msForBuilder = silenceDuration;
                    for (int i = 0; i < msForBuilder / 65535; ++i)
                    {
                        builder.AppendBreak(new TimeSpan(655350000));
                        msForBuilder -= 65535;
                    }
                    builder.AppendBreak(new TimeSpan(msForBuilder * 10000));
                }
                msAtStart = 0;
                if (progress != null)
                {
                    progress.Report(75*subtitle.Index/subtitles.Count);
                }
            }
            if (useSox)
            {
                int forCounter = subtitles.Count / MaxFilesForOneCommand;
                if (subtitles.Count % MaxFilesForOneCommand > 0)
                {
                    ++forCounter;
                }
                var finalFileName = string.Empty;
                var parameters = new List<string>();
                for (int i = 0; i < forCounter; ++i)
                {
                    parameters.Clear();
                    for (int j = 0; j < MaxFilesForOneCommand && i * MaxFilesForOneCommand + j < subtitles.Count; ++j)
                    {
                        var fileName = subtitles[i * MaxFilesForOneCommand + j].Index;
                        var outputFileName = subtitles[i * MaxFilesForOneCommand + j].Index + "_2.wav";
                        parameters.Add(outputFileName);
                        if (subtitles[i * MaxFilesForOneCommand + j].Index == 1)
                        {
                            CommandExecutor.ExecuteSilenceCommand(tempDirectory + Path.DirectorySeparatorChar + fileName, silences[0], silences[1], tempDirectory + Path.DirectorySeparatorChar + outputFileName);
                        }
                        else
                        {
                            CommandExecutor.ExecuteSilenceCommand(tempDirectory + Path.DirectorySeparatorChar + fileName, 0, silences[subtitles[i * MaxFilesForOneCommand + j].Index], tempDirectory + Path.DirectorySeparatorChar + outputFileName);
                        }
                        tempFiles.Add(tempDirectory + Path.DirectorySeparatorChar + outputFileName);
                        if (progress != null)
                        {
                            progress.Report(20 * subtitles[i * MaxFilesForOneCommand + j].Index / subtitles.Count+75);
                        }
                    }
                    finalFileName = "final" + (i + 1) + ".wav";
                    parameters.Add(finalFileName);
                    CommandExecutor.ExecuteConcatFilesCommand(parameters, tempDirectory);
                    tempFiles.Add(finalFileName);
                }
                parameters.Clear();
                for (int i = 0; i < forCounter; ++i)
                {
                    var fileName = "final" + (i + 1) + ".wav";
                    parameters.Add(fileName);
                }
                finalFileName = "final.wav";
                parameters.Add(finalFileName);
                CommandExecutor.ExecuteConcatFilesCommand(parameters, tempDirectory);
                if (progress != null)
                {
                    progress.Report(100);
                }
                }
                else
            {
                _speechService.Speak(builder, tempDirectory + Path.DirectorySeparatorChar + "final");
            }
            FileUtils.RemoveFiles(tempFiles);
        }

        private long SpeakSubtitle(string text, string outputFile, long duration = -1)
        {
            int chosenRate = _speechService.GetRate();
            var validDuration = true;
            var shortened = false;
            do
            {
                _speechService.Speak(text, outputFile);
                if (duration >= 0)
                {
                    long fileDuration = FileUtils.GetAudioFileDuration(outputFile);
                    if (fileDuration > duration)
                    {
                        validDuration = false;
                        if (_speechService.GetRate() == 10)
                        {
                            if (!shortened)
                            {
                                _speechService.SetRate(chosenRate);
                                text = text.RemovePunctuation().Shorten();
                                shortened = true;
                            }
                            else
                            {
                                return fileDuration - duration;
                            }
                        }
                        else
                        {
                            _speechService.IncreaseRate();
                        }
                    }
                    else if (!validDuration)
                    {
                        validDuration = true;
                        _speechService.SetRate(chosenRate);
                    }
                }
            }
            while (!validDuration);
            return 0;
        }

        private long CalculateSpeechTime(long previousSilence, long overlap)
        {
            if (overlap > previousSilence)
            {
                return 0;
            }
            else if (previousSilence >= 2 * overlap)
            {
                return 2 * overlap;
            }
            return overlap;
        }

    }
}
