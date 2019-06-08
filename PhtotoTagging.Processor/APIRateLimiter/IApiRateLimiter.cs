using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PhotoTagging.Processor.APIRateLimiter
{
    public interface IApiRateLimiter
    {
        Task AwaitRateLimitAsync();
    }
}
