using System.Speech.Synthesis;
using System.Speech.AudioFormat;

namespace SubtitleDubber.Utils
{
    public class SpeechUtils
    {
        private static SpeechSynthesizer _synthesizer;
        private SpeechAudioFormatInfo _synthFormat;

        public SpeechUtils()
        {
            _synthesizer = new SpeechSynthesizer();
_synthFormat = new (44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo);

        }

        public List<VoiceInfo> GetInstalledVoices()
        {
            List<VoiceInfo> voices = new List<VoiceInfo>();
                         foreach (var voice in _synthesizer.GetInstalledVoices())
                        {
                            var info = voice.VoiceInfo;
                voices.Add(info);
             }
            return voices;
        }

        public VoiceInfo GetVoice()
        {
            return _synthesizer.Voice;
        }

        public void SetVoice(string name)
        {
            _synthesizer.SelectVoice(name);
                    }

public void Speak(string text)
        {
            _synthesizer.Pause();
            _synthesizer.SpeakAsyncCancelAll();
            _synthesizer.Resume();
            _synthesizer.SpeakAsync(text);
        }

        public void SpeakToFile(string text, string fileName)
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

public void IncreaseRate()
        {
            ++_synthesizer.Rate;
        }

        public void SetRateToDefault()
        {
            _synthesizer.Rate = 0;
        }

        public int GetRate()
        {
            return _synthesizer.Rate;
        }
    }
}
