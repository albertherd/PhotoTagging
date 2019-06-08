using PhotoTagging.Processor;
using PhotoTagging.Processor.APIRateLimiter;
using PhotoTagging.Processor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PhotoTagging.Processor
{
    public class PhotoTaggingProcessor
    {
        private readonly BufferBlock<PhotoAnalysisRequest> _requestQueue;
        private readonly BufferBlock<PhotoAnalysisResult> _resultQueue;
        private readonly List<Action> _requestQueueListeners;
        private readonly PhotoAnayser _photoAnayser;
        private readonly IApiRateLimiter _rateLimiter;
        private const int ListenerCount = 5;

        public PhotoTaggingProcessor()
        {
            _requestQueue = new BufferBlock<PhotoAnalysisRequest>();
            _resultQueue = new BufferBlock<PhotoAnalysisResult>();
            _requestQueueListeners = new List<Action>();
            _photoAnayser = new PhotoAnayser();
            _rateLimiter = new F0RateLimiter();

            AddListenersToQueue();
        }

        public async Task<bool> EnqueueAsync(PhotoAnalysisRequest photo)
        {
            _requestQueue.Post(photo);
            return await Task.FromResult(true);
        }

        public void SignalReadyFromQueuing()
        {
            _requestQueue.Complete();
        }
       
        public void SubscribeToPhotoAnalysisComplete(ITargetBlock<PhotoAnalysisResult> targetBlock)
        {
            _resultQueue.LinkTo(targetBlock, new DataflowLinkOptions());
        }

        private void AddListenersToQueue()
        {
            _requestQueue.LinkTo(new ActionBlock<PhotoAnalysisRequest>(DoAnalysePhotoLoop, new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 5
            }));
        }

        private async Task DoAnalysePhotoLoop(PhotoAnalysisRequest photo)
        { 
            Console.WriteLine($"Requesting - {photo.FullPath}");
            var result = await _photoAnayser.AnalysePhoto(photo);

            if(result.Success)
            {
                Console.WriteLine($"Successful reply for - {photo.FullPath}");
                await _resultQueue.SendAsync(result);
            }
            else
            {
                Console.WriteLine($"Request failed for - {photo.FullPath}");
                await _rateLimiter.AwaitRateLimitAsync();
                Console.WriteLine($"Contiuing on - {photo.FullPath}");
                await EnqueueAsync(photo);
            }
            
        }
    }
}
