using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubtitleDubber.Models;
using SubtitleDubber.Helpers;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Runtime.CompilerServices;
using SubtitleDubber.Models.Commands;

namespace SubtitleDubber.Utils
{
    public class AudioUtils
    {

        private long GetFileDuration(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            long length = fileInfo.Length;
            double fileLength = length;
            double audioDuration = fileLength / 176400 * 1000;                 // time = FileLength / (Sample Rate * Channels * Bits per sample /8)

            long fileDuration = (long)audioDuration;
            return fileDuration;
        }

        private void RemoveFile(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            fileInfo.Delete();
        }

        private void RenameFile(string oldName, string newName)
        {
            File.Move(oldName, newName);
        }
        
        public void CreateSubtitleFiles(List<SubtitleItem> subtitles, string output, bool useSox = true)
        {
            string subtitleText;
            long fileDuration, msAtStart=0, subtitleSpeechDuration, silenceDuration;
            var builder = new PromptBuilder();
            var silences = new long[subtitles.Count + 1];
                                    foreach (var subtitle in subtitles)
                                    {
                subtitleText = subtitle.Text.RemoveAllFormatting();
                                        var fileName = output + "\\" + subtitle.Index + ".wav";
                if (subtitle.Index == subtitles.Count)
                {
                    subtitleSpeechDuration = (long)subtitle.Duration.TotalActiveTime.TotalMilliseconds;
                }
                else
                {
                    subtitleSpeechDuration = (long)subtitles[subtitle.Index].Duration.Start.Subtract(subtitle.Duration.Start).TotalMilliseconds;
                }
                var overlap = SpeakSubtitle(subtitleText, fileName, subtitleSpeechDuration);
                if (overlap>0)
                {
                    var silenceCutValue = CalculateSpeechTime(silences[subtitle.Index - 1], overlap);
                    if (silenceCutValue == 0)
                    {

                    }
                    else
                    {
                        subtitleSpeechDuration += silenceCutValue;
                        silences[subtitle.Index - 1]-=silenceCutValue;
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
                fileDuration = GetFileDuration(fileName);
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
            }
            if (useSox)
            {
                int forCounter = subtitles.Count / 500;
                if (subtitles.Count % 500 > 0)
                {
                    ++forCounter;
                }
                var finalFileName = string.Empty;
                var parameters = new List<string>();
                for (int i = 0; i < forCounter; ++i)
                {
                    parameters.Clear();
                    for (int j = 0; j < 500 && i * 500 + j < subtitles.Count; ++j)
                    {
                        var fileName = subtitles[i * 500 + j].Index + ".wav";
                        var outputFileName = subtitles[i * 500 + j].Index + "_2.wav";
                        parameters.Add(outputFileName);
                        if (subtitles[i * 500 + j].Index == 1)
                        {
CommandExecutor.ExecuteSilenceCommand(output + "\\"+fileName, silences[0], silences[1], output+"\\"+outputFileName);
                                                    }
                        else
                        {
                            CommandExecutor.ExecuteSilenceCommand(output + "\\" + fileName, 0, silences[subtitles[i * 500 + j].Index], output + "\\" + outputFileName);
                                                    }
                    }
                    finalFileName = "final" + (i + 1) + ".wav";
                    parameters.Add(finalFileName);
                    CommandExecutor.ExecuteConcatFilesCommand(parameters, output);
                }
                parameters.Clear();
                for (int i = 0; i < forCounter; ++i)
                {
                    var fileName = "final" + (i + 1) + ".wav";
                    parameters.Add(fileName);
                }
                finalFileName = "final.wav";
                parameters.Add(finalFileName);
                CommandExecutor.ExecuteConcatFilesCommand(parameters, output);
                for (int i = 0; i < forCounter; ++i)
                {
                    RemoveFile(output + "\\" + "final" + (i + 1) + ".wav");
                }
            }
            else
            {
                SpeechUtils.SpeakPrompt(builder, output + "\\final.wav");
            }
                        foreach (var subtitle in subtitles)
                        {
                            var fileName = output + "\\" + subtitle.Index + ".wav";
                            var outputFileName = output + "\\" + subtitle.Index + "_2.wav";
                                        RemoveFile(fileName);
if (useSox)
                {
                                            RemoveFile(outputFileName);
                }
            }
        }

            private long SpeakSubtitle(string text, string outputFile, long duration = -1)
        {
            int chosenRate = SpeechUtils.GetRate();
            var validDuration = true;
            var shortened = false;
            do
            {
                SpeechUtils.SpeakToFile(text, outputFile);
                if (duration >= 0)
                {
                    long fileDuration = GetFileDuration(outputFile);
                    if (fileDuration > duration)
                    {
                        validDuration = false;
                        if (SpeechUtils.GetRate() == 10)
                        {
                            if (!shortened)
                            {
                                SpeechUtils.SetRate(chosenRate);
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
                            SpeechUtils.IncreaseRate();
                        }
                    }
                    else if (!validDuration)
                    {
                        validDuration = true;
                        SpeechUtils.SetRate(chosenRate);
                    }
                }
            }
                        while (!validDuration);
            return 0;
        }

        public List<SubtitleStreamDescription> GetSubtitleList(string videoFileName)
        {
var commandOutput = CommandExecutor.ExecuteSubtitleListCommand(videoFileName);
            var subtitles = commandOutput.Split("\r\n");
            var descriptionList = new List<SubtitleStreamDescription>();

foreach(var subtitle in subtitles)
            {
                if (!string.IsNullOrEmpty(subtitle))
                {
                    var description = new SubtitleStreamDescription();
                    var subtitleParts = subtitle.Split(",");
                    description.Id = long.Parse(subtitleParts[0]);
                    description.LanguageCode = subtitleParts[1];
if (subtitleParts.Length == 3)
                    {
                        description.Title = subtitleParts[2];
                                            }
                    descriptionList.Add(description);
                }
            }
            return descriptionList;
        }

    private long CalculateSpeechTime(long previousSilence, long overlap)
        {
            if (overlap>previousSilence)
            {
                return 0;
            }
else if (previousSilence>=2*overlap)
            {
                return 2 * overlap;
            }
            return overlap;
        }

        public void DownloadSubtitle(string inputVideoFileName, string outputSubtitleFileName, string subtitleFormat, int subtitleTrackId)
        {
            CommandExecutor.ExecuteDownloadSubtitleCommand(inputVideoFileName, outputSubtitleFileName, subtitleFormat, subtitleTrackId);
        }

    }
}
