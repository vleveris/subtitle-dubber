using SubtitleDubber.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleDubber.Validators
{
    public class SubtitlesValidator
    {
        public const int FirstIndex = 1;

public bool Validate(List<SubtitleItem> subtitles)
    {
            var validIndexes = AreIndexesValid(subtitles);
            var areOverlaps = AreOverlaps(subtitles);
            return validIndexes && !areOverlaps;
                }

    private bool AreIndexesValid(List<SubtitleItem> subtitles)
    {
        if (subtitles == null)
        {
            throw new ArgumentNullException(nameof(subtitles));
        }

        var expectedIndex = FirstIndex;
                    foreach (var subtitle in subtitles)
        {
            if (subtitle.Index != expectedIndex)
                {
                    return false;
                }

                ++expectedIndex;
        }

        return true;
    }

        private bool AreOverlaps(List<SubtitleItem> subtitles)
        {
            if (subtitles == null)
            {
                throw new ArgumentNullException(nameof(subtitles));
                            }

            for (var i = 0; i < subtitles.Count; ++i)
            {
                var subtitle = subtitles[i];

                SubtitleItem nextSubtitle = null;
                if (i+1<subtitles.Count)
                {
                    nextSubtitle = subtitles[i + 1];
                }
                                if (nextSubtitle != null)
                {
                    if (subtitle.Duration.End > nextSubtitle.Duration.Start)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
