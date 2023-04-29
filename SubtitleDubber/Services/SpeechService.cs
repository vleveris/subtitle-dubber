using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using SubtitleDubber.Models;

namespace SubtitleDubber.Services
{
    public class SpeechService
    {
        private SpeechSynthesizer _synthesizer = new();
        private SpeechAudioFormatInfo _synthFormat = new(44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo);
public const int MaxRate = 10, MinRate = -10, DefaultRate = MinRate + MaxRate, MinVolume = 0, MaxVolume = 100;

        public List<Models.VoiceInfo> GetInstalledVoices()
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

        public string GetVoiceName()
        {
            return _synthesizer.Voice.Name;
        }

        public void SetVoice(string name)
        {
            try
            {
                _synthesizer.SelectVoice(name);
            }
            catch (ArgumentException)
            {
                throw;
            }
        }

        public void Speak(string text)
        {
            try
            {
                _synthesizer.SetOutputToDefaultAudioDevice();
            }
            catch
            {

            }
            _synthesizer.Pause();
            _synthesizer.SpeakAsyncCancelAll();
            _synthesizer.Resume();
                _synthesizer.SpeakAsync(text);
        }

        public void Speak(string text, string fileName)
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

        public void Speak(PromptBuilder builder, string fileName)
        {
            _synthesizer.SetOutputToWaveFile(fileName, _synthFormat);
            _synthesizer.Speak(builder);
            _synthesizer.SetOutputToNull();
        }

        public void IncreaseRate()
        {
if (_synthesizer.Rate < MaxRate)
            {
                ++_synthesizer.Rate;
            }
        }

        public void DecreaseRate()
        {
            if (_synthesizer.Rate > MinRate)
            {
                --_synthesizer.Rate;
            }
        }

        public void SetRateToDefault()
        {
            _synthesizer.Rate = DefaultRate;
        }

        public int GetRate()
        {
            return _synthesizer.Rate;
        }

        public void SetRate(int rate)
        {
if (rate > MaxRate || rate < MinRate)
            {
                throw new ArgumentOutOfRangeException("Rate must be between " + MinRate + " and " + MaxRate + ".");
            }
            _synthesizer.Rate = rate;
        }

        public int GetVolume()
        {
            return _synthesizer.Volume;
        }

        public void SetVolume(int volume)
        {
            if (volume > MaxRate || volume < MinVolume)
            {
                throw new ArgumentOutOfRangeException("Volume must be between " + MinVolume + " and " + MaxVolume + ".");
            }
            _synthesizer.Volume = volume;
        }

    }
}
