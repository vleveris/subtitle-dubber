using System.Speech.Synthesis;
using System.Speech.AudioFormat;

namespace SubtitleDubber.Utils
{
    class SpeechUtils
    {
        private SpeechSynthesizer _synthesizer;
        private SpeechAudioFormatInfo _synthFormat;

        public SpeechUtils()
        {
            _synthesizer = new SpeechSynthesizer();
_synthFormat = new (44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo);

        }

        public void SpeakToFile(string text, string fileName)
        {
            /*            foreach (var voice in synthesizer.GetInstalledVoices())
                        {
                            var info = voice.VoiceInfo;
                            Console.WriteLine($"Id: {info.Id} | Name: {info.Name} |                               Age: { info.Age} | Gender: { info.Gender} | Culture: { info.Culture}                ");
             }
            */
            _synthesizer.SelectVoice("Regina");
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
