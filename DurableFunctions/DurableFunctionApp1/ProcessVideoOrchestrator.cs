using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctionApp1
{
    public static class ProcessVideoOrchestrator
    {
        [FunctionName("ProcessVideoOrchestrator")]
        public static async Task<object> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context, ILogger log)
        {
            var videoLocation = context.GetInput<string>();

            if (!context.IsReplaying)
                log.LogInformation($"Before transcoding video.");

            var transcodedLocation = await context.CallActivityAsync<string>("ProcessVideoOrchestrator_TranscodeVideo", videoLocation);

            if (!context.IsReplaying)
                log.LogInformation($"Before extracting thumbnail.");

            var thumbnailLocation = await context.CallActivityAsync<string>("ProcessVideoOrchestrator_ExtractThumbnail", transcodedLocation);

            if (!context.IsReplaying)
                log.LogInformation($"Before prepending intro.");

            var withIntroLocation = await context.CallActivityAsync<string>("ProcessVideoOrchestrator_PrependIntro", transcodedLocation);

            return new
            {
                Transcoded = transcodedLocation,
                Thumbnail = thumbnailLocation,
                WithIntro = withIntroLocation
            };
        }

        [FunctionName("ProcessVideoOrchestrator_TranscodeVideo")]
        public static async Task<string> TranscodeVideo([ActivityTrigger] string inputVideo, ILogger log)
        {
            log.LogInformation($"Transcoding {inputVideo}.");

            await Task.Delay(5000);

            return "transcoded.mp4";
        }

        [FunctionName("ProcessVideoOrchestrator_ExtractThumbnail")]
        public static async Task<string> ExtractThumbnail([ActivityTrigger] string inputVideo, ILogger log)
        {
            log.LogInformation($"Extracting Thumbnail {inputVideo}.");

            await Task.Delay(5000);

            return "thumbnail.png";
        }

        [FunctionName("ProcessVideoOrchestrator_PrependIntro")]
        public static async Task<string> PrependIntro([ActivityTrigger] string inputVideo, ILogger log)
        {
            log.LogInformation($"Appending intro to video {inputVideo}.");

            await Task.Delay(5000);

            return "withIntro.mp4";
        }

        [FunctionName("ProcessVideoOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            dynamic data = await req.Content.ReadAsAsync<object>();

            var video = data?.video;

            if (video == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass the video location the query string or in the request body");                
            }

            // Function input comes from the request content.
            log.LogInformation($"About to start orchestration for {video}.");

            string instanceId = await starter.StartNewAsync("ProcessVideoOrchestrator", video);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}