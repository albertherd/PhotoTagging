using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using PhotoTagging.Processor;
using PhtotoTagging.Processor;
using PhotoTagging.Processor.Models;
using System.Threading.Tasks.Dataflow;
using System.Threading;

namespace PhotoTagging
{
    class Program
    {
        static int photosToProcess = 0;
        static int photosProcessed = 0;
        static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        private static PhotoEnumerator _photoEnumerator;
        private static PhotoTaggingProcessor _photoTaggingProcessor;
        private static ExifProcessor _exifProcessor;

        static async Task Main(string[] args)
        {
            Initialize();
            Console.WriteLine("Folder path for photo tagging:");
            await AnalysePhotosDirectoryAsync(Console.ReadLine());
        }

        static void Initialize()
        {
            _photoEnumerator = new PhotoEnumerator();
            _exifProcessor = new ExifProcessor();
            _photoTaggingProcessor = new PhotoTaggingProcessor();
            _photoTaggingProcessor.SubscribeToPhotoAnalysisComplete(new ActionBlock<PhotoAnalysisResult>(OnPhotoProcessingDone));
        }

        static async Task AnalysePhotosDirectoryAsync(string directory)
        {
            List<PhotoAnalysisRequest> photos =  _photoEnumerator.EnumerateDirectory(directory, false).ToList();
            photosToProcess = photos.Count;
            foreach (PhotoAnalysisRequest photo in photos)
            {
                await _photoTaggingProcessor.EnqueueAsync(photo);
                Console.WriteLine($"Queued {photo.FullPath}");
            }
            WaitForPhotosToFinishProcessing();
        }

        static void WaitForPhotosToFinishProcessing()
        {
            autoResetEvent.WaitOne();
            Console.WriteLine("Processing done!");
            Console.ReadLine();
        }

        static void OnPhotoProcessingDone(PhotoAnalysisResult photoAnalysisResult)
        {
            _exifProcessor.WriteExifMetaData(photoAnalysisResult.PhotoAnalysisRequest.FullPath, photoAnalysisResult);
            photosProcessed++;

            if (photosProcessed == photosToProcess)
                autoResetEvent.Set();
        }
    }
}
