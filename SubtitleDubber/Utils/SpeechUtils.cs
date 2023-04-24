using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using SubtitleDubber.Models;
namespace SubtitleDubber.Utils
{
    public static class SpeechUtils
    {
        private static SpeechSynthesizer _synthesizer = new();
        private static SpeechAudioFormatInfo _synthFormat = new(44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo);

        public static List<Models.VoiceInfo> GetInstalledVoices()
        {
            List<Models.VoiceInfo> voices = new List<Models.VoiceInfo>();
                         foreach (var voice in _synthesizer.GetInstalledVoices())
                        {
                            var info = voice.VoiceInfo;
                var infoModel = new Models.VoiceInfo();
                infoModel.AdditionalInfo = info.AdditionalInfo;
                infoModel.Name = info.Name;
                infoModel.Description = info.Description;
                infoModel.Age = info.Age.ToString();
                infoModel.Language = info.Culture.ToString();
                infoModel.Id = info.Id;
                infoModel.Gender = info.Gender.ToString();

                voices.Add(infoModel);
             }
            return voices;
        }

        public static string GetVoiceName()
        {
            return _synthesizer.Voice.Name;
        }

        public static void SetVoice(string name)
        {
            _synthesizer.SelectVoice(name);
                    }

public static void Speak(string text)
        {
            _synthesizer.SetOutputToDefaultAudioDevice();
            _synthesizer.Pause();
            _synthesizer.SpeakAsyncCancelAll();
            _synthesizer.Resume();
            _synthesizer.SpeakAsync(text);
        }

        public static void SpeakToFile(string text, string fileName)
        {
                _synthesizer.SetOutputToWaveFile(fileName, _synthFormat);
            try
            {
                _synthesizer.Speak(text);
            }
            catch (Exception x)
            {
                Thread.Sleep(1);
            }
            _synthesizer.SetOutputToNull();
        }

        public static void SpeakPrompt(PromptBuilder builder, string fileName)
        {
            _synthesizer.SetOutputToWaveFile(fileName, _synthFormat);
            _synthesizer.Speak(builder);
            _synthesizer.SetOutputToNull();
        }

        public static void IncreaseRate()
        {
            ++_synthesizer.Rate;
        }

        public static void SetRateToDefault()
        {
            _synthesizer.Rate = 0;
        }

        public static int GetRate()
        {
            return _synthesizer.Rate;
        }

        public static void SetRate(int rate)
        {
                _synthesizer.Rate = rate;
        }

        public static void SetVolume(int volume)
        {
            _synthesizer.Volume = volume;
        }

    }
}
