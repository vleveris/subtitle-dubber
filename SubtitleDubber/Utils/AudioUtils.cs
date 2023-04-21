using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubtitleDubber.Models;
using SubtitleDubber.Helpers;
using System.Diagnostics;

namespace SubtitleDubber.Utils
{
    internal class AudioUtils
    {
public static string InputFilePath { set; get; }

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

        public void CreateSubtitleFiles(List<SubtitleItem> subtitles, string output)
        {
            string subtitleText;
            long fileDuration, subtitleDuration, silenceDuration, msAtStart=0;
                                    foreach (var subtitle in subtitles)
                                    {
                subtitleText = subtitle.Text.RemoveAllFormatting();
                                        var fileName = output + "\\" + subtitle.Index + ".wav";
                                        var outputFileName = output + "\\" + subtitle.Index + "_2.wav";
                                        SpeakSubtitle(subtitleText, fileName, (long)subtitle.Duration.TotalActiveTime.TotalMilliseconds);
                if (subtitle.Index == 1)
                {
                    msAtStart = (long)subtitle.Duration.Start.TotalMilliseconds;
                }
                fileDuration = GetFileDuration(fileName);
                if (subtitle.Index < subtitles.Count)
                {
                    silenceDuration = (long)(subtitles[subtitle.Index].Duration.Start.Subtract(subtitle.Duration.Start).TotalMilliseconds-fileDuration);
                }
                else
                {
                    silenceDuration = (long)(subtitle.Duration.TotalActiveTime.TotalMilliseconds - fileDuration);
                }
                AddSilence(fileName, msAtStart, silenceDuration, outputFileName);
                msAtStart = 0;
            }
            int forCounter = subtitles.Count / 500;
            if (subtitles.Count%500>0)
            {
                ++forCounter;
            }
            var finalFileName = string.Empty;
            string parameters = string.Empty;
                        for (int i = 0; i < forCounter; ++i)
                        {
                parameters = string.Empty;
                for (int j=0;j<500 && i*500+j<subtitles.Count;++j)
                            {
                                var fileName = subtitles[i* 500 + j].Index + "_2.wav";
                                parameters= parameters+ fileName + " ";
                                                }
                            finalFileName = "final"+(i+1)+".wav";
                            parameters= parameters+ finalFileName;
                CommandExecutor.Execute("sox", parameters, output);
                        }
            parameters = string.Empty;
                        for (int i = 0; i < forCounter; ++i)
                        {
                            var fileName = "final" + (i + 1) + ".wav";
                            parameters= parameters+ fileName + " ";
                        }
                        finalFileName = "final.wav";
                        parameters= parameters+ finalFileName;
            CommandExecutor.Execute("sox", parameters, output);
                        for (int i = 0; i < forCounter; ++i)
                        {
                            RemoveFile(output + "\\" + "final" + (i + 1) + ".wav");
                        }
            foreach (var subtitle in subtitles)
            {
                var fileName = output + "\\" + subtitle.Index + ".wav";
                var outputFileName = output + "\\" + subtitle.Index + "_2.wav";
                            RemoveFile(fileName);
                                RemoveFile(outputFileName);
            }

            }

            private void SpeakSubtitle(string text, string outputFile, long duration)
        {
            var validDuration = true;
            do
            {
                SpeechUtils.SpeakToFile(text, outputFile);
                long fileDuration = GetFileDuration(outputFile);
                if (fileDuration > duration)
                {
                    validDuration = false;
                    if (SpeechUtils.GetRate() == 10)
                    {
                        SpeechUtils.SetRateToDefault();
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
                    SpeechUtils.SetRateToDefault();
                }
            }
            while (!validDuration);
        }

    }
}
