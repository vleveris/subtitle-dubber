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
using System.Diagnostics;

namespace SubtitleDubber.Services
{
    public class DubbingService
    {
        private SpeechService _speechService = new();
        private SubtitleService _subtitleService = new();
        private const string TemporarySubtitleFileName = "subtitle.srt", WaveFileExtension = ".wav", FileWithSilenceNameEnd = "_2", FinalFileNameStart = "final";
        private const int MaxFilesForOneCommand = 500;
        private List<string> _tempFiles = new List<string>();
        private readonly List<ISubtitleParser> _supportedParsers =
            new List<ISubtitleParser>();
        private string _tempDirectoryName;
        private long[] _silences;

        public void Dub(int subtitleTrackId, string inputVideoFileName, string outputVideoFileName, bool useSox, IProgress<int> progress)
        {
            //        _tempDirectoryName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _tempDirectoryName = "C:\\hardas\\SubtitleDubber";
            FileUtils.CreateDirectory(_tempDirectoryName);
            var inputSubtitleFileName = _tempDirectoryName + Path.DirectorySeparatorChar + TemporarySubtitleFileName;
            _subtitleService.DownloadSubtitle(inputVideoFileName, inputSubtitleFileName, FileFormat.DefaultSubtitleFileExtension, subtitleTrackId);
            _tempFiles.Add(inputSubtitleFileName);
            Dub(inputSubtitleFileName, outputVideoFileName, useSox, progress);
        }

        public void Dub(string inputSubtitleFileName, string outputVideoFileName, bool useSox, IProgress<int> progress)
        {
if (string.IsNullOrEmpty(_tempDirectoryName))
            {
                //        _empDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                _tempDirectoryName = "C:\\hardas\\SubtitleDubber";
                Directory.CreateDirectory(_tempDirectoryName);
            }

            FillParsers();
            if (!FileUtils.Exists(inputSubtitleFileName))
            {

            }
            var selectedParser = _supportedParsers.Where(parser => parser.FileExtension == Path.GetExtension(inputSubtitleFileName))
                .Select(parser => parser).FirstOrDefault();

            var subtitles = selectedParser.Parse(inputSubtitleFileName);

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
            FileUtils.RemoveFiles(_tempFiles);
        }

        private void DubSubtitles(List<SubtitleItem> subtitles, IProgress<int> progress)
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
                if (progress != null)
                {
                    progress.Report(75 * subtitle.Index / subtitles.Count);
                }
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
                    long fileDuration = FileUtils.GetAudioFileDuration(outputFileName);
                    if (fileDuration > maxPossibleDuration)
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

        private void FillParsers()
        {
            _supportedParsers.Add(new SSAParser());
            _supportedParsers.Add(new VTTParser());
            _supportedParsers.Add(new SrtParser());
        }

private void DubSubtitle(SubtitleItem currentSubtitle, SubtitleItem nextSubtitle)
        {
            long subtitleSpeechDuration;
            var subtitleText = currentSubtitle.Text.RemoveAllFormatting();
            var subtitleIndex = currentSubtitle.Index;
            var fileName = _tempDirectoryName + Path.DirectorySeparatorChar + subtitleIndex;
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
            var fileDuration = FileUtils.GetAudioFileDuration(fileName);
            var silenceDuration = subtitleSpeechDuration - fileDuration;
                _silences[subtitleIndex] = silenceDuration;
        }

        private void InsertSilences(IProgress<int> progress)
        {
            for (int i = 1; i < _silences.Length; ++i)
            {
                var inputFileName = _tempDirectoryName + Path.DirectorySeparatorChar + i;
                var outputFileName = inputFileName + FileWithSilenceNameEnd + WaveFileExtension;
                if (i == 1)
                {
                    CommandExecutor.ExecuteSilenceCommand(inputFileName, outputFileName, _silences[0], _silences[1]);
                }
                else
                {
                    CommandExecutor.ExecuteSilenceCommand(inputFileName, outputFileName, 0, _silences[i]);
                }
                _tempFiles.Add(outputFileName);
                if (progress != null)
                {
                    progress.Report(20 * i/ _silences.Length-1+ 75);
                }
            }
        }

        private void ConcatFiles(int numberOfFiles, IProgress<int> progress)
        {
            var forCounter = numberOfFiles / MaxFilesForOneCommand;
            int fileNumber = 0;
            if (numberOfFiles % MaxFilesForOneCommand > 0)
            {
                ++forCounter;
            }
            var outputFileName = string.Empty;
            var inputFileName = string.Empty;
            var parameters = new List<string>();

            for (int i = 0; i < forCounter; ++i)
            {
                parameters.Clear();
                for (int j = 0; j < MaxFilesForOneCommand && i * MaxFilesForOneCommand + j < numberOfFiles; ++j)
                {
                    fileNumber = i * MaxFilesForOneCommand + j + 1;
                    inputFileName = fileNumber + FileWithSilenceNameEnd + WaveFileExtension;
                    parameters.Add(inputFileName);
                }
                outputFileName = FinalFileNameStart + i + WaveFileExtension;
                parameters.Add(outputFileName);
                CommandExecutor.ExecuteConcatFilesCommand(parameters, _tempDirectoryName);
                _tempFiles.Add(_tempDirectoryName + Path.DirectorySeparatorChar + outputFileName);
            }
            parameters.Clear();
            for (int i = 0; i < forCounter; ++i)
            {
                inputFileName = FinalFileNameStart + i + WaveFileExtension;
                parameters.Add(inputFileName);
            }
            outputFileName = FinalFileNameStart + WaveFileExtension;
            parameters.Add(outputFileName);
            CommandExecutor.ExecuteConcatFilesCommand(parameters, _tempDirectoryName);
            if (progress != null)
            {
                progress.Report(100);
            }
        }

        private void GenerateAudioTrackFromBuilder(IProgress<int> progress)
        {
            var builder = new PromptBuilder();
if (_silences.Length > 0)
            {
                AppendSilence(_silences[0], builder);
            }
for (int i=1; i<_silences.Length; ++i)
            {
                builder.AppendAudio(_tempDirectoryName + Path.DirectorySeparatorChar + i);
                AppendSilence(_silences[i], builder);
            }
            _speechService.Speak(builder, _tempDirectoryName + Path.DirectorySeparatorChar + FinalFileNameStart + WaveFileExtension);
            if (progress != null)
            {
                progress.Report(100);
            }
        }

        private void AppendSilence(long ms, PromptBuilder builder)
        {
            for (int i = 0; i < ms/ 65535; ++i)
            {
                builder.AppendBreak(new TimeSpan(655350000));
                ms -= 65535;
            }
            builder.AppendBreak(new TimeSpan(ms* 10000));
        }

}
}
