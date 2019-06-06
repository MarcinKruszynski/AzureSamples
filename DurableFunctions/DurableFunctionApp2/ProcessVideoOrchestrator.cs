using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctionApp2
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
            string approvalResult = "Unknown";

            try
            {
                var bitRates = new[] { 1000, 2000, 3000, 4000 };
                var transcodeTasks = new List<Task<VideoFileInfo>>();

                foreach(var bitRate in bitRates)
                {
                    var info = new VideoFileInfo { Location = videoLocation, BitRate = bitRate };
                    var task = context.CallActivityAsync<VideoFileInfo>("ProcessVideoOrchestrator_TranscodeVideo", info);
                    transcodeTasks.Add(task);
                }

                var transcodeResults = await Task.WhenAll(transcodeTasks);

                transcodedLocation = transcodeResults
                    .OrderByDescending(r => r.BitRate)
                    .Select(r => r.Location)
                    .First();                

                if (!context.IsReplaying)
                    log.LogInformation($"Before extracting thumbnail.");

                thumbnailLocation = await context.CallActivityWithRetryAsync<string>(
                                    "ProcessVideoOrchestrator_ExtractThumbnail",
                                    new RetryOptions(TimeSpan.FromSeconds(5), 2) { Handle = ex => ex is InvalidOperationException },
                                    transcodedLocation);

                if (!context.IsReplaying)
                    log.LogInformation($"Before prepending intro.");

                withIntroLocation = await context.CallActivityAsync<string>("ProcessVideoOrchestrator_PrependIntro", transcodedLocation);

                await context.CallActivityAsync("ProcessVideoOrchestrator_SendApprovalRequestEmail", withIntroLocation);


                using (var cts = new CancellationTokenSource())
                {
                    var timeoutAt = context.CurrentUtcDateTime.AddSeconds(30);
                    var timeoutTask = context.CreateTimer(timeoutAt, cts.Token);
                    var approvalTask = context.WaitForExternalEvent<string>("ApprovalResult");

                    var winner = await Task.WhenAny(approvalTask, timeoutTask);
                    if (winner == approvalTask)
                    {
                        approvalResult = approvalTask.Result;
                        cts.Cancel();
                    }
                    else
                    {
                        approvalResult = "Timed Out";
                    }
                }                

                if (approvalResult == "Approved")
                {
                    await context.CallActivityAsync("ProcessVideoOrchestrator_PublishVideo", withIntroLocation);
                }
                else
                {
                    await context.CallActivityAsync("ProcessVideoOrchestrator_RejectVideo", withIntroLocation);
                }
            }
            catch (Exception e)
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
                WithIntro = withIntroLocation,
                ApprovalResult = approvalResult
            };
        }

        [FunctionName("ProcessVideoOrchestrator_TranscodeVideo")]
        public static async Task<VideoFileInfo> TranscodeVideo([ActivityTrigger] VideoFileInfo inputVideo, ILogger log)
        {
            log.LogInformation($"Transcoding {inputVideo.Location} to {inputVideo.BitRate}.");

            await Task.Delay(inputVideo.BitRate);

            var transcodedLocation = $"{Path.GetFileNameWithoutExtension(inputVideo.Location)}-{inputVideo.BitRate}kbps.mp4";

            return new VideoFileInfo
            {
                Location = transcodedLocation,
                BitRate = inputVideo.BitRate
            };
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

        [FunctionName("ProcessVideoOrchestrator_SendApprovalRequestEmail")]
        public static async Task SendApprovalRequestEmail([ActivityTrigger] string inputVideo, ILogger log)
        {
            log.LogInformation($"Requesting approval for {inputVideo}.");

            await Task.Delay(10000);            
        }

        [FunctionName("ProcessVideoOrchestrator_PublishVideo")]
        public static async Task PublishVideo([ActivityTrigger] string inputVideo, ILogger log)
        {
            log.LogInformation($"Publishing {inputVideo}.");

            await Task.Delay(10000);
        }

        [FunctionName("ProcessVideoOrchestrator_RejectVideo")]
        public static async Task RejectVideo([ActivityTrigger] string inputVideo, ILogger log)
        {
            log.LogInformation($"Rejecting {inputVideo}.");

            await Task.Delay(10000);
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




        [FunctionName("ProcessVideoOrchestrator_StartPeriodicTask")]
        public static async Task<HttpResponseMessage> StartPeriodicTask(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient client,
            ILogger log)
        {
            var instanceId = await client.StartNewAsync("ProcessVideoOrchestrator_PeriodicTask", 0);
            return client.CreateCheckStatusResponse(req, instanceId);          
        }


        [FunctionName("ProcessVideoOrchestrator_PeriodicTask")]
        public static async Task<int> PeriodicTask(
            [OrchestrationTrigger] DurableOrchestrationContext context, ILogger log)
        {
            var timesRun = context.GetInput<int>();

            timesRun++;

            if (!context.IsReplaying)
                log.LogInformation($"Starting periodic task activity {context.InstanceId}, {timesRun}");

            await context.CallActivityAsync("ProcessVideoOrchestrator_PeriodicActivity", timesRun);

            var nextRun = context.CurrentUtcDateTime.AddSeconds(30);
            await context.CreateTimer(nextRun, CancellationToken.None);

            context.ContinueAsNew(timesRun);

            return timesRun;
        }

        [FunctionName("ProcessVideoOrchestrator_PeriodicActivity")]
        public static void PeriodicActivity([ActivityTrigger] int timesRun, ILogger log)
        {
            log.LogInformation($"Running the periodic activity, times run = {timesRun}");            
        }
    }
}