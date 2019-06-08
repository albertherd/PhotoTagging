using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoTagging.Processor.APIRateLimiter
{
    public class F0RateLimiter : IApiRateLimiter
    {
        private readonly TimeSpan IntervalLength = TimeSpan.FromSeconds(60);

        public async Task AwaitRateLimitAsync()
        {
            await Task.Delay(IntervalLength);
        }
    }
}
