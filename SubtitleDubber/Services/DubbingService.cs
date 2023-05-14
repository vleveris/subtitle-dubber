using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubtitleDubber.Models;
using SubtitleDubber.Helpers;
using SubtitleDubber.Utils;
using System.Speech.Synthesis;
using System.Collections;
using System.Diagnostics;
using SubtitleDubber.Validators;
using System.Reflection;

namespace SubtitleDubber.Services
{
    public class DubbingService
    {
        public string VoiceName { get; set; }
        public int VoiceRate { get; set; }
        public int VoiceVolume { get; set; }

        private SpeechService _speechService = new();
        private SubtitleService _subtitleService = new();
        private const string TemporarySubtitleFileName = "subtitle.srt", WaveFileExtension = ".wav", FileWithSilenceNameEnd = "_2", FinalFileNameStart = "final";
        private const int MaxFilesForOneCommand = 500;
        private string _tempDirectoryName;
        private long[] _silences;
        private FileUtils _fileUtils = new();
        private List<string> _tempFiles = new List<string>();

        public void Dub(int subtitleTrackId, string inputVideoFileName, string outputVideoFileName, bool useSox, int delay, int originalTrackVolume, IProgress<string> progress)
        {
            _tempDirectoryName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _fileUtils.CreateDirectory(_tempDirectoryName);
            var inputSubtitleFileName = Path.Combine(_tempDirectoryName, TemporarySubtitleFileName);
            _subtitleService.DownloadSubtitle(inputVideoFileName, inputSubtitleFileName, FileFormat.DefaultSubtitleFileExtension, subtitleTrackId);
            _tempFiles.Add(inputSubtitleFileName);
            Dub(inputSubtitleFileName, inputVideoFileName, outputVideoFileName, useSox, delay, originalTrackVolume, progress);
        }

        public void Dub(string inputSubtitleFileName, string inputVideoFileName, string outputVideoFileName, bool useSox, int delay, int originalTrackVolume, IProgress<string> progress)
        {
            if (string.IsNullOrEmpty(_tempDirectoryName))
            {
                _tempDirectoryName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                _fileUtils.CreateDirectory(_tempDirectoryName);
            }
            _speechService.SetVoice(VoiceName);
            _speechService.SetRate(VoiceRate);
            _speechService.SetVolume(VoiceVolume);

            if (!_fileUtils.Exists(inputSubtitleFileName))
            {
                throw new ArgumentException("Subtitle file name doesn't exist.");
            }
            var subtitles = _subtitleService.GetSubtitlesFromFile(inputSubtitleFileName);
            DubSubtitles(subtitles, progress);
                            if (useSox)
            {
                InsertSilences(progress);
                ConcatFiles(subtitles.Count, progress);
                }
                else
            {
                GenerateAudioTrackFromBuilder(progress);
            }

            var inputAudioFileName = Path.Combine(_tempDirectoryName, FinalFileNameStart + WaveFileExtension);
            MergeAudioTracks(inputVideoFileName, inputAudioFileName, outputVideoFileName, delay, originalTrackVolume, progress);

                        _fileUtils.RemoveFiles(_tempFiles);
            _fileUtils.RemoveDirectory(_tempDirectoryName);
            _tempFiles.Clear();
            _tempDirectoryName = null;
        }

        private void DubSubtitles(List<SubtitleItem> subtitles, IProgress<string> progress)
        {
            _silences = new long[subtitles.Count + 1];
            foreach (var subtitle in subtitles)
            {
                SubtitleItem nextSubtitle = null;
                if (subtitle.Index < subtitles.Count)
                {
                    nextSubtitle = subtitles[subtitle.Index];
                }
                DubSubtitle(subtitle, nextSubtitle);
                    ReportProgress(progress, 100 * subtitle.Index / subtitles.Count, "Generating audio subtitle files...");
            }
        }

        private long SpeakSubtitle(string text, string outputFileName, long maxPossibleDuration = -1)
        {
            int chosenRate = _speechService.GetRate();
            var validDuration = true;
            var shortened = false;
            do
            {
                _speechService.Speak(text, outputFileName);
                if (maxPossibleDuration >= 0)
                {
                    long fileDuration = _fileUtils.GetAudioFileDuration(outputFileName);
                    if (fileDuration > maxPossibleDuration)
                    {
                        validDuration = false;
                        if (_speechService.GetRate() == SpeechService.MaxRate)
                        {
                            if (!shortened)
                            {
                                _speechService.SetRate(chosenRate);
                                text = text.RemovePunctuation().Shorten();
                                shortened = true;
                            }
                            else
                            {
                                return fileDuration - maxPossibleDuration;
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

        private long IncreaseSubtitleSpeechDuration(long previousSilence, long overlap)
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


private void DubSubtitle(SubtitleItem currentSubtitle, SubtitleItem nextSubtitle)
        {
            long subtitleSpeechDuration;
            var subtitleText = currentSubtitle.Text.RemoveAllFormatting();
            var subtitleIndex = currentSubtitle.Index;
            var fileName = Path.Combine(_tempDirectoryName, subtitleIndex.ToString());
            var isFirstSubtitle = subtitleIndex == 1;
            var isLastSubtitle = nextSubtitle == null;

            if (isLastSubtitle)
            {
                subtitleSpeechDuration = (long)currentSubtitle.Duration.TotalActiveTime.TotalMilliseconds;
            }
            else
            {
                subtitleSpeechDuration = (long)nextSubtitle.Duration.Start.Subtract(currentSubtitle.Duration.Start).TotalMilliseconds;
            }
            var overlap = SpeakSubtitle(subtitleText, fileName, subtitleSpeechDuration);
            _tempFiles.Add(fileName);
            if (overlap > 0)
            {
                var silenceCutValue = IncreaseSubtitleSpeechDuration(_silences[subtitleIndex - 1], overlap);
                if (silenceCutValue == 0)
                {

                }
                else
                {
                    subtitleSpeechDuration += silenceCutValue;
                    _silences[subtitleIndex - 1] -= silenceCutValue;
                }
            }
            if (isFirstSubtitle)
            {
                    _silences[0] = (long)currentSubtitle.Duration.Start.TotalMilliseconds;
                }
            var fileDuration = _fileUtils.GetAudioFileDuration(fileName);
            var silenceDuration = subtitleSpeechDuration - fileDuration;
                _silences[subtitleIndex] = silenceDuration;
        }

        private void InsertSilences(IProgress<string> progress)
        {
            for (var i = 1; i < _silences.Length; ++i)
            {
                var inputFileName = Path.Combine(_tempDirectoryName, i.ToString());
                var outputFileName = inputFileName + FileWithSilenceNameEnd + WaveFileExtension;
                var executor = new CommandExecutor();
                if (i == 1)
                {
                    executor.ExecuteSilenceCommand(inputFileName, outputFileName, _silences[0], _silences[1]);
                }
                else
                {
                    executor.ExecuteSilenceCommand(inputFileName, outputFileName, 0, _silences[i]);
                }
                _tempFiles.Add(outputFileName);
                    ReportProgress(progress, 100 * i/ (_silences.Length-1), "Adding silences...");
            }
        }

        private void ConcatFiles(int numberOfFiles, IProgress<string> progress)
        {
            var progressString = "Generating subtitle audio track...";
            var forCounter = numberOfFiles / MaxFilesForOneCommand;
            int fileNumber = 0;
            if (numberOfFiles % MaxFilesForOneCommand > 0)
            {
                ++forCounter;
            }
            var outputFileName = string.Empty;
            var inputFileName = string.Empty;
            var parameters = new List<string>();
            var executor = new CommandExecutor();
                ReportProgress(progress, 0, progressString);
            for (var i = 0; i < forCounter; ++i)
            {
                parameters.Clear();
                for (var j = 0; j < MaxFilesForOneCommand && i * MaxFilesForOneCommand + j < numberOfFiles; ++j)
                {
                    fileNumber = i * MaxFilesForOneCommand + j + 1;
                    inputFileName = fileNumber + FileWithSilenceNameEnd + WaveFileExtension;
                    parameters.Add(inputFileName);
                }
                outputFileName = FinalFileNameStart + i + WaveFileExtension;
                parameters.Add(outputFileName);
                executor.ExecuteConcatFilesCommand(parameters, _tempDirectoryName);
                _tempFiles.Add(Path.Combine(_tempDirectoryName, outputFileName));
                    ReportProgress(progress, 80*fileNumber/numberOfFiles, progressString);
                            }
            parameters.Clear();
            for (var i = 0; i < forCounter; ++i)
            {
                inputFileName = FinalFileNameStart + i + WaveFileExtension;
                parameters.Add(inputFileName);
            }
            outputFileName = FinalFileNameStart + WaveFileExtension;
            parameters.Add(outputFileName);
            executor.ExecuteConcatFilesCommand(parameters, _tempDirectoryName);
            _tempFiles.Add(Path.Combine(_tempDirectoryName, outputFileName));
            ReportProgress(progress, 100, progressString);
        }

        private void GenerateAudioTrackFromBuilder(IProgress<string> progress)
        {
            var progressString = "Generating subtitle audio track...";
                ReportProgress(progress, 0, progressString);
            var builder = new PromptBuilder();
if (_silences.Length > 0)
            {
                AppendSilence(_silences[0], builder);
            }
for (var i=1; i<_silences.Length; ++i)
            {
                builder.AppendAudio(Path.Combine(_tempDirectoryName, i.ToString()));
                AppendSilence(_silences[i], builder);
            }
            _speechService.Speak(builder, Path.Combine(_tempDirectoryName, FinalFileNameStart + WaveFileExtension));
                ReportProgress(progress, 100, progressString);
        }

        private void AppendSilence(long ms, PromptBuilder builder)
        {
            for (var i = 0; i < ms/ 65535; ++i)
            {
                builder.AppendBreak(new TimeSpan(655350000));
                ms -= 65535;
            }
            builder.AppendBreak(new TimeSpan(ms* 10000));
        }

        private void ReportProgress(IProgress<string> progress, int percentage, string text)
        {
            if (progress != null)
            {
                progress.Report(text + Environment.NewLine + percentage);
            }
        }

        private void MergeAudioTracks(string inputVideoFileName, string inputAudioFileName, string outputVideoFileName, int delay, int originalTrackVolume, IProgress<string> progress)
        {
            var executor = new CommandExecutor();
            var mergeActivityText = "Merging audio tracks...";

            int audioDuration = -1, videoDuration = -1, longerDuration = -1;
                        var process = executor.ExecuteMergeAudioCommand(inputVideoFileName, inputAudioFileName, outputVideoFileName, delay, originalTrackVolume);
            ReportProgress(progress, 0, mergeActivityText);

            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    var outputParts = e.Data.Split(" ");
                    if ( e.Data.Trim().StartsWith("Duration:"))
                    {
                        if (outputParts.Length > 1)
                    {
                            var index = 1;
                        while ((string.IsNullOrEmpty(outputParts[index]) || outputParts[index] == "Duration:") && index < outputParts.Length)
                        {
                            ++index;
                        }
                            if (videoDuration == -1)
                            {
                                videoDuration = ParseTime(outputParts[index]);
                            }
                            else
                            {
                                audioDuration = ParseTime(outputParts[index]);
                                }
                            if (audioDuration > videoDuration)
                            {
                                longerDuration = audioDuration;
                            }
                            else
                            {
                                longerDuration = videoDuration;
                            }
                            longerDuration += delay / 1000 + 1;
                        }
                    }
else if (outputParts[0].StartsWith ("frame="))
                    {
                        var index = 1;
                        while (!outputParts[index].StartsWith("time=") && index < outputParts.Length)
                        {
                            ++index;
                        }
       var seconds = ParseTime(outputParts[index].Replace("time=", ""));
                        ReportProgress(progress, seconds* 100 / longerDuration, mergeActivityText);
                    }

                }

                }
            );

            process.Start();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Dispose();
            ReportProgress(progress, 100, mergeActivityText);
        }

        private int ParseTime(string time)
    {
    var totalSeconds = 0;
    var timeParts = time.Replace(".", ":").Split(":");
    if (timeParts.Length > 3)
    {
        int hours = 0, minutes = 0, seconds = 0;
        int.TryParse(timeParts[0], out hours);
        int.TryParse(timeParts[1], out minutes);
        int.TryParse(timeParts[2], out seconds);
if (hours>=0)
                {
                    totalSeconds = hours * 60 * 60 + minutes * 60 + seconds;
                }
            }

    return totalSeconds;
    }
    }
}
