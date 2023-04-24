using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using System.Security.Principal;

namespace SubtitleDubber.Utils
{
    public static class SpeechUtils
    {
        private static SpeechSynthesizer _synthesizer = new();
        private static SpeechAudioFormatInfo _synthFormat = new(44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo);

        public static List<VoiceInfo> GetInstalledVoices()
        {
            List<VoiceInfo> voices = new List<VoiceInfo>();
                         foreach (var voice in _synthesizer.GetInstalledVoices())
                        {
                            var info = voice.VoiceInfo;
                voices.Add(info);
             }
            return voices;
        }

        public static VoiceInfo GetVoice()
        {
            return _synthesizer.Voice;
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
