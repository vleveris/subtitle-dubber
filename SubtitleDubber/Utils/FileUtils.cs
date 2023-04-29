using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleDubber.Utils
{
    public class FileUtils
    {
        public long GetAudioFileDuration(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            long length = fileInfo.Length;
            double fileLength = length;
            var audioDuration = fileLength / 176400 * 1000;                 // time = FileLength / (Sample Rate * Channels * Bits per sample /8)
                        var fileDuration = (long)audioDuration;
            return fileDuration;
        }

        public void RemoveFile(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            fileInfo.Delete();
        }

                public void RemoveFiles(List<string> fileNames)
        {
            foreach (var fileName in fileNames)
            {
                RemoveFile(fileName);
                            }
        }

        public void RenameFile(string oldName, string newName)
        {
            File.Move(oldName, newName);
        }

        public void CreateDirectory(string directoryName)
        {
            Directory.CreateDirectory(directoryName);
        }

        public void RemoveDirectory(string directoryName)
        {
            Directory.Delete(directoryName);
        }

        public bool Exists(string fileName)
        {
            return File.Exists(fileName);
        }

    }
}
