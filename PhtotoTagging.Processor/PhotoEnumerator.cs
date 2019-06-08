using PhotoTagging.Processor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PhtotoTagging.Processor
{
    public class PhotoEnumerator
    {
        static readonly string[] supportedImagesPattern = new[] { ".JPG", ".JPEG" };

        public IEnumerable<PhotoAnalysisRequest> EnumerateDirectory(string path, bool recursive = true)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            return EnumerateDirectory(directory, recursive);
        }

        public IEnumerable<PhotoAnalysisRequest> EnumerateDirectory(DirectoryInfo directory, bool recursive = true)
        {
            List<FileInfo> files = directory.EnumerateFiles().Where(file => supportedImagesPattern.Contains(file.Extension.ToUpperInvariant())).ToList();

            foreach (FileInfo file in files)
            {
                yield return new PhotoAnalysisRequest()
                {
                    FullPath = file.FullName,
                };
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    foreach (PhotoAnalysisRequest photoAnalysisRequest in EnumerateDirectory(subDirectory, recursive))
                    {
                        yield return photoAnalysisRequest;
                    }
                }
            }
        }
    }
}
