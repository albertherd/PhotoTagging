using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using PhotoTagging.Processor.Models;

namespace PhotoTagging.Processor
{
    internal class PhotoAnayser
    {
        const string subscriptionKey = "86e013658bfa47adbfac4c8f316c51e5";
        const string uriBase = "https://westeurope.api.cognitive.microsoft.com/vision/v2.0/analyze";

        const string visualFeatures = "visualFeatures=Tags,Description";       
        const string uri = uriBase + "?" + visualFeatures;       
        const string subscriptionHeaderKeyName = "Ocp-Apim-Subscription-Key";
        readonly string[] supportedImagesPattern = new[] { ".JPG", ".JPEG" };

        readonly HttpClientHandler _httpClientHandler;
        readonly HttpClient _client;

        internal PhotoAnayser()
        {
            _httpClientHandler = new HttpClientHandler();
            _client = new HttpClient(_httpClientHandler, false);
            _client.DefaultRequestHeaders.Add(subscriptionHeaderKeyName, subscriptionKey);
        }

        internal async Task<PhotoAnalysisResult> AnalysePhoto(PhotoAnalysisRequest request)
        {
            //_rateLimiter.EnsureRequestWithinRateLimitsAsync(imageFullPath);
            HttpResponseMessage httpResponseMessage = await DoHttpCall(request);

            return httpResponseMessage.IsSuccessStatusCode
                ? await GetSuccessPhotoAnalysisResult(request, httpResponseMessage)
                : new PhotoAnalysisResult();
        }

        internal async Task<HttpResponseMessage> DoHttpCall(PhotoAnalysisRequest photo)
        {
            ByteArrayContent content = new ByteArrayContent(GetImageAsByteArray(photo.FullPath));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return await _client.PostAsync(uri, content);
        }

        internal async Task<PhotoAnalysisResult> GetSuccessPhotoAnalysisResult(PhotoAnalysisRequest request, HttpResponseMessage message)
        {
            return new PhotoAnalysisResult()
            {
                PhotoAnalysisRequest = request,
                Data = JsonConvert.DeserializeObject<PhotoAnalysisResultData>(await message.Content.ReadAsStringAsync()),
                Success = true
            };            
        }

        private byte[] GetImageAsByteArray(string imageFilePath)
        {
            const int maxWidth = 1280;

            using (MemoryStream memoryStream = new MemoryStream())
            using (Image<Rgba32> image = Image.Load(imageFilePath))
            {
                if(image.Width > maxWidth)
                {
                    float downsizeRatio = image.Width / maxWidth;
                    image.Mutate(ctx => ctx.Resize((int)(image.Width / downsizeRatio), (int)(image.Height / downsizeRatio)));
                }

                image.Save(memoryStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
                return memoryStream.ToArray();
            }
            
        }
    }
}