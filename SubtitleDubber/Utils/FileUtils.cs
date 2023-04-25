using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleDubber.Utils
{
    public static class FileUtils
    {
        public static long GetAudioFileDuration(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            long length = fileInfo.Length;
            double fileLength = length;
            var audioDuration = fileLength / 176400 * 1000;                 // time = FileLength / (Sample Rate * Channels * Bits per sample /8)
                        var fileDuration = (long)audioDuration;
            return fileDuration;
        }

        public static void RemoveFile(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            fileInfo.Delete();
        }

        public static void RenameFile(string oldName, string newName)
        {
            File.Move(oldName, newName);
        }

    }
}
