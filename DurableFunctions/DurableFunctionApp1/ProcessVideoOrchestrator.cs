using System;
using System.Collections.Generic;
using System.IO;
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

            string transcodedLocation = null;
            string thumbnailLocation = null;
            string withIntroLocation = null;

            try
            {

                transcodedLocation = await context.CallActivityAsync<string>("ProcessVideoOrchestrator_TranscodeVideo", videoLocation);

                if (!context.IsReplaying)
                    log.LogInformation($"Before extracting thumbnail.");

                thumbnailLocation = await context.CallActivityWithRetryAsync<string>(
                                    "ProcessVideoOrchestrator_ExtractThumbnail",
                                    new RetryOptions(TimeSpan.FromSeconds(5), 2) { Handle = ex => ex is InvalidOperationException },
                                    transcodedLocation);

                if (!context.IsReplaying)
                    log.LogInformation($"Before prepending intro.");

                withIntroLocation = await context.CallActivityAsync<string>("ProcessVideoOrchestrator_PrependIntro", transcodedLocation);
            }
            catch(Exception e)
            {
                if (!context.IsReplaying)
                    log.LogInformation($"Caught an error from an activity: {e.Message}.");

                return new
                {
                    Error = "Failed to process uploaded video",
                    Message = e.Message
                };
            }

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

            await Task.Delay(50000);

            return $"{Path.GetFileNameWithoutExtension(inputVideo)}-transcoded.mp4";
        }

        [FunctionName("ProcessVideoOrchestrator_ExtractThumbnail")]
        public static async Task<string> ExtractThumbnail([ActivityTrigger] string inputVideo, ILogger log)
        {
            log.LogInformation($"Extracting Thumbnail {inputVideo}.");

            if (inputVideo.Contains("error"))
            {
                throw new InvalidOperationException("Could not extract thumbnail");
            }

            await Task.Delay(50000);

            return "thumbnail.png";
        }

        [FunctionName("ProcessVideoOrchestrator_PrependIntro")]
        public static async Task<string> PrependIntro([ActivityTrigger] string inputVideo, ILogger log)
        {
            log.LogInformation($"Appending intro to video {inputVideo}.");

            await Task.Delay(50000);

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