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
        
        private void AddSilence(string inputFileName, long msAtStart, long msAtEnd, string outputFileName)
        {
            string parameters = inputFileName + " " + outputFileName + " pad " + msAtStart / 1000;
            if (msAtStart%1000 != 0)
            {
                parameters = parameters+ ".";
                long msRemainder = msAtStart % 1000;
if (msRemainder<10)
                {
                    parameters= parameters+ "00";
                                    }
                else if (msRemainder < 100)
                {
                    parameters= parameters+ "0";
                }
                parameters= parameters+ msRemainder;
                            }

            parameters= parameters+ " " + msAtEnd / 1000;
            if (msAtEnd%1000 != 0)
            {
                parameters= parameters+ ".";
                long msRemainder = msAtEnd % 1000;
                if (msRemainder < 10)
                {
                    parameters= parameters+ "00";
                }
                else if (msRemainder < 100)
                {
                    parameters= parameters+ "0";
                }
                parameters= parameters+ msRemainder;
            }
            CommandExecutor.Execute("sox", parameters);
                    }

        public void CreateSubtitleFiles(List<SubtitleItem> subtitles, string output, bool useSox = true)
        {
            string subtitleText;
            long fileDuration, msAtStart=0, subtitleSpeechDuration, silenceDuration;
            var builder = new PromptBuilder();
                                    foreach (var subtitle in subtitles)
                                    {
                subtitleText = subtitle.Text.RemoveAllFormatting();
                                        var fileName = output + "\\" + subtitle.Index + ".wav";
                                        var outputFileName = output + "\\" + subtitle.Index + "_2.wav";
                if (subtitle.Index == subtitles.Count)
                {
                    subtitleSpeechDuration = (long)subtitle.Duration.TotalActiveTime.TotalMilliseconds;
                }
                else
                {
                    subtitleSpeechDuration = (long)subtitles[subtitle.Index].Duration.Start.Subtract(subtitle.Duration.Start).TotalMilliseconds;
                }
                SpeakSubtitle(subtitleText, fileName, subtitleSpeechDuration);
                if (subtitle.Index == 1)
                {
                    msAtStart = (long)subtitle.Duration.Start.TotalMilliseconds;
                    if (!useSox)
                    {
                        builder.AppendBreak(new TimeSpan(0, 0, 0, (int)msAtStart / 1000, (int)msAtStart % 1000));
                    }
                }
                fileDuration = GetFileDuration(fileName);
                silenceDuration = subtitleSpeechDuration - fileDuration;
                if (useSox)
                {
                    AddSilence(fileName, msAtStart, silenceDuration, outputFileName);
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
                string parameters = string.Empty;
                for (int i = 0; i < forCounter; ++i)
                {
                    parameters = string.Empty;
                    for (int j = 0; j < 500 && i * 500 + j < subtitles.Count; ++j)
                    {
                        var fileName = subtitles[i * 500 + j].Index + "_2.wav";
                        parameters = parameters + fileName + " ";
                    }
                    finalFileName = "final" + (i + 1) + ".wav";
                    parameters = parameters + finalFileName;
                    CommandExecutor.Execute("sox", parameters, output);
                }
                parameters = string.Empty;
                for (int i = 0; i < forCounter; ++i)
                {
                    var fileName = "final" + (i + 1) + ".wav";
                    parameters = parameters + fileName + " ";
                }
                finalFileName = "final.wav";
                parameters = parameters + finalFileName;
                CommandExecutor.Execute("sox", parameters, output);
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

            private void SpeakSubtitle(string text, string outputFile, long duration = -1)
        {
            int chosenRate = SpeechUtils.GetRate();
            var validDuration = true;
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
                            SpeechUtils.SetRate(chosenRate);
                            text = text.RemovePunctuation().Shorten();

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
        }

        public List<SubtitleStreamDescription> GetSubtitleList(string videoFileName)
        {
            var commandName = "ffprobe";
            var parameters = "-loglevel error -select_streams s -show_entries stream=index:stream_tags=language:stream_tags=title -of csv=p=0 " + videoFileName;
var commandOutput = CommandExecutor.Execute(commandName, parameters);
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

        public void DownloadSubtitle(string inputVideoFileName, string outputSubtitleFileName, string subtitleFormat, long subtitleTrackId)
        {
            var command = "ffmpeg";
            var parameters = "-i " + inputVideoFileName + " -map 0:" + subtitleTrackId + " -c:s " + subtitleFormat + " " + outputSubtitleFileName + " -y";
            CommandExecutor.Execute(command, parameters);
                    }

private string InsertSilence(string parameters, long ms)
{
    var insertion = string.Empty + ms / 1000;
    long msRemainder = ms % 1000;
    if (msRemainder!= 0)
    {
        insertion= insertion+ ".";
        if (msRemainder < 10)
        {
            insertion= insertion+ "00";
        }
        else if (msRemainder < 100)
        {
            insertion= insertion+ "0";
        }
        insertion= insertion+ msRemainder;
    }
    return parameters + insertion;
    }
    }
}
