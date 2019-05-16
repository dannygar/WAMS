/*

Azure Media Services REST API v2 Function
 
This function submits a job wth encoding and/or analytics.

Input:
{
    "assetId" : "nb:cid:UUID:2d0d78a2-685a-4b14-9cf0-9afb0bb5dbfc", // Mandatory, Id of the source asset
    "mes" :                 // Optional but required to encode with Media Encoder Standard (MES)
    {
        "preset" : "Content Adaptive Multiple Bitrate MP4", // Optional but required to encode with Media Encoder Standard (MES). If MESPreset contains an extension "H264 Multiple Bitrate 720p with thumbnail.json" then it loads this file from ..\Presets
        "outputStorage" : "amsstorage01" // Optional. Storage account name where to put the output asset (attached to AMS account)
    }
    "mesThumbnails" :      // Optional but required to generate thumbnails with Media Encoder Standard (MES)
    {
        "start" : "{Best}",  // Optional. Start time/mode. Default is "{Best}"
        "outputStorage" : "amsstorage01" // Optional. Storage account name where to put the output asset (attached to AMS account)
    }
    "mepw" :                // Optional but required to encode with Premium Workflow Encoder
    {
        "workflowAssetId" : "nb:cid:UUID:2d0d78a2-685a-4b14-9cf0-9afb0bb5dbfc", // Required. Id for the workflow asset
        "workflowConfig"  : "",                                                  // Optional. Premium Workflow Config for the task
        "outputStorage" : "amsstorage01" // Optional. Storage account name where to put the output asset (attached to AMS account)
    },
    "indexV1" :             // Optional but required to index audio with Media Indexer v1
    {
        "language" : "English", // Optional. Default is "English"
        "outputStorage" : "amsstorage01" // Optional. Storage account name where to put the output asset (attached to AMS account)
    },
    "indexV2" :             // Optional but required to index audio with Media Indexer v2
    {
        "language" : "EnUs", // Optional. Default is EnUs
        "outputStorage" : "amsstorage01" // Optional. Storage account name where to put the output asset (attached to AMS account)
    },
    "ocr" :             // Optional but required to do OCR
    {
        "language" : "AutoDetect", // Optional (Autodetect is the default)
        "outputStorage" : "amsstorage01" // Optional. Storage account name where to put the output asset (attached to AMS account)
    },
    "faceDetection" :             // Optional but required to do Face Detection
    {
        "mode" : "PerFaceEmotion", // Optional (PerFaceEmotion is the default)
        "outputStorage" : "amsstorage01" // Optional. Storage account name where to put the output asset (attached to AMS account)
    },
    "faceRedaction" :             // Optional but required to do Face Redaction
    {
        "mode" : "analyze"                  // Optional (analyze is the default)
        "outputStorage" : "amsstorage01"    // Optional. Storage account name where to put the output asset (attached to AMS account)
    },
     "motionDetection" :             // Optional but required to do Motion Detection
    {
        "level" : "medium",                 // Optional (medium is the default)
        "outputStorage" : "amsstorage01"    // Optional. Storage account name where to put the output asset (attached to AMS account)
    },
     "summarization" :                      // Optional but required to do Motion Detection
    {
        "duration" : "0.0",                 // Optional (0.0 is the default)
        "outputStorage" : "amsstorage01"    // Optional. Storage account name where to put the output asset (attached to AMS account)
    },
     "videoAnnotation" :             // Optional but required to do Video Annotator
    {
        "outputStorage" : "amsstorage01"    // Optional. Storage account name where to put the output asset (attached to AMS account)
    },

    // General job properties
    "priority" : 10,                            // Optional, priority of the job
    "useEncoderOutputForAnalytics" : true,      // Optional, use generated asset by MES or Premium Workflow as a source for media analytics
    "jobName" : ""                              // Optional, job name  

    // For compatibility only with old workflows. Do not use anymore!
    "mesPreset" : "Adaptive Streaming",         // Optional but required to encode with Media Encoder Standard (MES). If MESPreset contains an extension "H264 Multiple Bitrate 720p with thumbnail.json" then it loads this file from ..\Presets
    "workflowAssetId" : "nb:cid:UUID:2d0d78a2-685a-4b14-9cf0-9afb0bb5dbfc", // Optional, but required to encode the asset with Premium Workflow Encoder. Id for the workflow asset
    "workflowConfig"  : ""                      // Optional. Premium Workflow Config for the task
    "indexV1Language" : "English",              // Optional but required to index the asset with Indexer v1
    "indexV2Language" : "EnUs",                 // Optional but required to index the asset with Indexer v2
    "ocrLanguage" : "AutoDetect" or "English",  // Optional but required to do OCR
    "faceDetectionMode" : "PerFaceEmotion,      // Optional but required to trigger face detection
    "faceRedactionMode" : "analyze",            // Optional, but required for face redaction
    "motionDetectionLevel" : "medium",          // Optional, required for motion detection
    "summarizationDuration" : "0.0"             // Optional. Required to create video summarization. "0.0" for automatic
}

Output:
{
    "jobId" :  // job id
    "otherJobsQueue" = 3 // number of jobs in the queue
    "mes" : // Output asset generated by MES (if mesPreset was specified)
        {
            assetId : "",
            taskId : ""
        },
    "mesThumbnails" :// Output asset generated by MES
        {
            assetId : "",
            taskId : ""
        },
    "mepw" : // Output asset generated by Premium Workflow Encoder
        {
            assetId : "",
            taskId : ""
        },
    "indexV1" :  // Output asset generated by Indexer v1
        {
            assetId : "",
            taskId : "",
            language : ""
        },
    "indexV2" : // Output asset generated by Indexer v2
        {
            assetId : "",
            taskId : "",
            language : ""
        },
    "ocr" : // Output asset generated by OCR
        {
            assetId : "",
            taskId : ""
        },
    "faceDetection" : // Output asset generated by Face detection
        {
            assetId : ""
            taskId : ""
        },
    "faceRedaction" : // Output asset generated by Face redaction
        {
            assetId : ""
            taskId : ""
        },
     "motionDetection" : // Output asset generated by motion detection
        {
            assetId : "",
            taskId : ""
        },
     "summarization" : // Output asset generated by video summarization
        {
            assetId : "",
            taskId : ""
        },
     "videoAnnotation" :// Output asset generated by Video Annotator
        {
            assetId : "",
            taskId : ""
        }
 }
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Web.Http;
using AMS.V3;
using AMS.V3.Models;
using AMS.V3.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.MediaServices.Client;
using Newtonsoft.Json;
using JobState = Microsoft.WindowsAzure.MediaServices.Client.JobState;

namespace WAMS.Functions
{
    public static class SubmitJob
    {
        // Field for service context.
        //private static CloudMediaContext _context = null;
        private static IAzureMediaServicesClient _amsClient = null;
        private static IMediaUtilityService _mus = null;

        [FunctionName("SubmitJob")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, Microsoft.Azure.WebJobs.ExecutionContext execContext)
        {
            log.LogInformation("WebHook HTTP trigger SubmitJob function processed a request.");

            int taskindex = 0;
            bool useEncoderOutputForAnalytics = false;
            IAsset outputEncoding = null;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            log.LogInformation(requestBody);

            log.LogInformation($"asset id : {data.assetId}");

            if (data.assetId == null)
            {
                return new BadRequestObjectResult("Please pass asset ID in the input object (assetId)");
            }


            IJob job = null;
            ITask taskEncoding = null;

            int OutputMES = -1;
            int OutputMEPW = -1;
            int OutputIndex1 = -1;
            int OutputIndex2 = -1;
            int OutputOCR = -1;
            int OutputFaceDetection = -1;
            int OutputMotion = -1;
            int OutputSummarization = -1;
            int OutputFaceRedaction = -1;
            int OutputMesThumbnails = -1;
            int OutputVideoAnnotation = -1;
            int NumberJobsQueue = 0;

            MediaServicesCredentials amsCredentials = new MediaServicesCredentials();
            log.LogInformation($"Using Azure Media Service Rest API Endpoint : {amsCredentials.AmsRestApiEndpoint}");

            try
            {
                //AzureAdTokenCredentials tokenCredentials = new AzureAdTokenCredentials(amsCredentials.AmsAadTenantDomain,
                //                                new AzureAdClientSymmetricKey(amsCredentials.AmsClientId, amsCredentials.AmsClientSecret),
                //                                AzureEnvironments.AzureCloudEnvironment);

                //AzureAdTokenProvider tokenProvider = new AzureAdTokenProvider(tokenCredentials);

                //_context = new CloudMediaContext(amsCredentials.AmsRestApiEndpoint, tokenProvider);


                // Get Media Services Client
                _amsClient = await MediaServicesClientHelper.GetAzureMediaServicesClientAsync(amsCredentials);

                // Instantiate Media Utility Service
                _mus = new MediaUtilityService(amsCredentials, log);

                // find the Asset
                string assetid = (string)data.assetId;
                var asset = await _mus.GetAssetAsync(assetid);

                if (asset == null)
                {
                    log.LogInformation($"Asset not found {assetid}");
                    return new BadRequestObjectResult("Asset not found");
                }


                var presets = new List<Preset>();


                if (data.useEncoderOutputForAnalytics != null && ((bool)data.useEncoderOutputForAnalytics) && (data.mesPreset != null || data.mes != null))  // User wants to use encoder output for media analytics
                {
                    useEncoderOutputForAnalytics = (bool)data.useEncoderOutputForAnalytics;
                }


                // Declare a new encoding job with the Standard encoder
                int priority = 10;
                if (data.priority != null)
                {
                    priority = (int)data.priority;
                }


                //job = _context.Jobs.Create(((string)data.jobName) ?? "Azure Functions Job", priority);
                
                //1. Add Encoding
                if (data.mes != null || data.mesPreset != null)  // MES Task
                {
                    // Get a media processor reference, and pass to it the name of the 
                    // processor to use for the specific task.
                    //IMediaProcessor processorMES = MediaServicesHelper.GetLatestMediaProcessorByName(_context, "Media Encoder Standard");

                    string preset = (data.mes != null)? (string)data.mes.preset : (string)data.mesPreset; // Compatibility mode

                    presets.Add(preset != null
                        ? new BuiltInStandardEncoderPreset() {PresetName = preset}
                        : new BuiltInStandardEncoderPreset() {PresetName = EncoderNamedPreset.AdaptiveStreaming});

                    if (!string.IsNullOrEmpty(preset) && preset.ToUpper().EndsWith(".JSON"))
                    {
                        // Build the folder path to the preset
                        string presetPath = Path.Combine(System.IO.Directory.GetParent(execContext.FunctionDirectory).FullName, "presets", preset);
                        log.LogInformation("presetPath= " + presetPath);
                        preset = File.ReadAllText(presetPath);
                    }

                    // Create a task with the encoding details, using a string preset.
                    // In this case "H264 Multiple Bitrate 720p" system defined preset is used.
                    //taskEncoding = job.Tasks.AddNew("MES encoding task",
                    //   processorMES,
                    //   preset,
                    //   TaskOptions.None);

                    // Specify the input asset to be encoded.
                    //taskEncoding.InputAssets.Add(asset);
                    //OutputMES = taskindex++;

                    // Add an output asset to contain the results of the job. 
                    // This output is specified as AssetCreationOptions.None, which 
                    // means the output asset is not encrypted. 
                    //outputEncoding = taskEncoding.OutputAssets.AddNew(asset.Name + " MES encoded", JobHelpers.OutputStorageFromParam(data.mes), AssetCreationOptions.None);
                }

                //IAsset an_asset = useEncoderOutputForAnalytics ? outputEncoding : asset;

                ////////////////////////////////////////////////////
                // Media Analytics
                ////////////////////////////////////////////////////
                // indexV1
                if(!data.indexV2)
                    presets.Add(new VideoAnalyzerPreset(audioLanguage: data.indexV2.language, insightsToExtract: data.indexV2.insights));

                //OutputOCR = JobHelpers.AddTask(execContext, _context, job, an_asset, (data.ocr == null) ? (string)data.ocrLanguage : ((string)data.ocr.language ?? "AutoDetect"), "Azure Media OCR", "OCR.json", "AutoDetect", ref taskindex, specifiedStorageAccountName: JobHelpers.OutputStorageFromParam(data.ocr));
                //OutputFaceDetection = JobHelpers.AddTask(execContext, _context, job, an_asset, (data.faceDetection == null) ? (string)data.faceDetectionMode : ((string)data.faceDetection.mode ?? "PerFaceEmotion"), "Azure Media Face Detector", "FaceDetection.json", "PerFaceEmotion", ref taskindex, specifiedStorageAccountName: JobHelpers.OutputStorageFromParam(data.faceDetection));
                //OutputFaceRedaction = JobHelpers.AddTask(execContext, _context, job, an_asset, (data.faceRedaction == null) ? (string)data.faceRedactionMode : ((string)data.faceRedaction.mode ?? "comined"), "Azure Media Redactor", "FaceRedaction.json", "combined", ref taskindex, specifiedStorageAccountName: JobHelpers.OutputStorageFromParam(data.faceRedaction));
                //OutputMotion = JobHelpers.AddTask(execContext, _context, job, an_asset, (data.motionDetection == null) ? (string)data.motionDetectionLevel : ((string)data.motionDetection.level ?? "medium"), "Azure Media Motion Detector", "MotionDetection.json", "medium", ref taskindex, specifiedStorageAccountName: JobHelpers.OutputStorageFromParam(data.motionDetection));
                //OutputSummarization = JobHelpers.AddTask(execContext, _context, job, an_asset, (data.summarization == null) ? (string)data.summarizationDuration : ((string)data.summarization.duration ?? "0.0"), "Azure Media Video Thumbnails", "Summarization.json", "0.0", ref taskindex, specifiedStorageAccountName: JobHelpers.OutputStorageFromParam(data.summarization));
                //OutputVideoAnnotation = JobHelpers.AddTask(execContext, _context, job, an_asset, (data.videoAnnotation != null) ? "1.0" : null, "Azure Media Video Annotator", "VideoAnnotation.json", "1.0", ref taskindex, specifiedStorageAccountName: JobHelpers.OutputStorageFromParam(data.videoAnnotation));

                //// MES Thumbnails
                //OutputMesThumbnails = JobHelpers.AddTask(execContext, _context, job, asset, (data.mesThumbnails != null) ? ((string)data.mesThumbnails.Start ?? "{Best}") : null, "Media Encoder Standard", "MesThumbnails.json", "{Best}", ref taskindex, specifiedStorageAccountName: JobHelpers.OutputStorageFromParam(data.mesThumbnails));


                //Submit job
                await _mus.TransformAsync(new MediaTransformContext()
                {
                    JobName = (string)data.jobName ?? "Azure Functions Job",
                    InputAssetName = asset.Name,
                    TransformedAssetName = asset.Name + " MES encoded",
                    TransformName = asset.Name + " Transform",
                    ContainerName = data.mes.assetContainer,
                    OutputAssetContainer = "egress-videos",
                    Presets = presets
                });

                //job.Submit();
                log.LogInformation("Job Submitted");
                //NumberJobsQueue = _context.Jobs.Where(j => j.State == JobState.Queued).Count();
            }
            catch (Exception ex)
            {
                string message = ex.Message + ((ex.InnerException != null) ? Environment.NewLine + MediaServicesHelper.GetErrorMessage(ex) : "");
                log.LogInformation($"ERROR: Exception {message}");
                return new BadRequestErrorMessageResult(message);
            }

            //job = _context.Jobs.Where(j => j.Id == job.Id).FirstOrDefault(); // Let's refresh the job

            //log.LogInformation("Job Id: " + job.Id);
            //log.LogInformation("OutputAssetMESId: " + JobHelpers.ReturnId(job, OutputMES));
            //log.LogInformation("OutputAssetMEPWId: " + JobHelpers.ReturnId(job, OutputMEPW));
            //log.LogInformation("OutputAssetIndexV1Id: " + JobHelpers.ReturnId(job, OutputIndex1));
            //log.LogInformation("OutputAssetIndexV2Id: " + JobHelpers.ReturnId(job, OutputIndex2));
            //log.LogInformation("OutputAssetOCRId: " + JobHelpers.ReturnId(job, OutputOCR));
            //log.LogInformation("OutputAssetFaceDetectionId: " + JobHelpers.ReturnId(job, OutputFaceDetection));
            //log.LogInformation("OutputAssetFaceRedactionId: " + JobHelpers.ReturnId(job, OutputFaceRedaction));
            //log.LogInformation("OutputAssetMotionDetectionId: " + JobHelpers.ReturnId(job, OutputMotion));
            //log.LogInformation("OutputAssetSummarizationId: " + JobHelpers.ReturnId(job, OutputSummarization));
            //log.LogInformation("OutputMesThumbnailsId: " + JobHelpers.ReturnId(job, OutputMesThumbnails));
            //log.LogInformation("OutputAssetVideoAnnotationId: " + JobHelpers.ReturnId(job, OutputVideoAnnotation));

            return new OkObjectResult( new
            {
                jobId = job.Id,
                otherJobsQueue = NumberJobsQueue,
                mes = new
                {
                    assetId = JobHelpers.ReturnId(job, OutputMES),
                    taskId = JobHelpers.ReturnTaskId(job, OutputMES)
                },
                mepw = new
                {
                    assetId = JobHelpers.ReturnId(job, OutputMEPW),
                    taskId = JobHelpers.ReturnTaskId(job, OutputMEPW)
                },
                indexV1 = new
                {
                    assetId = JobHelpers.ReturnId(job, OutputIndex1),
                    taskId = JobHelpers.ReturnTaskId(job, OutputIndex1),
                    language = (string)data.indexV1Language
                },
                indexV2 = new
                {
                    assetId = JobHelpers.ReturnId(job, OutputIndex2),
                    taskId = JobHelpers.ReturnTaskId(job, OutputIndex2),
                    language = (string)data.indexV2Language
                },
                ocr = new
                {
                    assetId = JobHelpers.ReturnId(job, OutputOCR),
                    taskId = JobHelpers.ReturnTaskId(job, OutputOCR)
                },
                faceDetection = new
                {
                    assetId = JobHelpers.ReturnId(job, OutputFaceDetection),
                    taskId = JobHelpers.ReturnTaskId(job, OutputFaceDetection)
                },
                faceRedaction = new
                {
                    assetId = JobHelpers.ReturnId(job, OutputFaceRedaction),
                    taskId = JobHelpers.ReturnTaskId(job, OutputFaceRedaction)
                },
                motionDetection = new
                {
                    assetId = JobHelpers.ReturnId(job, OutputMotion),
                    taskId = JobHelpers.ReturnTaskId(job, OutputMotion)
                },
                summarization = new
                {
                    assetId = JobHelpers.ReturnId(job, OutputSummarization),
                    taskId = JobHelpers.ReturnTaskId(job, OutputSummarization)
                },
                mesThumbnails = new
                {
                    assetId = JobHelpers.ReturnId(job, OutputMesThumbnails),
                    taskId = JobHelpers.ReturnTaskId(job, OutputMesThumbnails)
                },
                videoAnnotation = new
                {
                    assetId = JobHelpers.ReturnId(job, OutputVideoAnnotation),
                    taskId = JobHelpers.ReturnTaskId(job, OutputVideoAnnotation)
                }
            });
        }
    }
}
