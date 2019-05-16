using FilmDove.Logger;
using Media.Utility.Data;
using Media.Utility.Helpers;
using Media.Utility.Models;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Media.Utility.Operations
{
    internal class MediaTransform
    {

        private MediaServices _mediaServices { get; set; }
        private AzureMediaServicesClient _azureMediaServicesClient { get; set; }

        private readonly IGraphLogger<MediaTransform> log = null;
        public MediaTransform(MediaServices mediaServices, IGraphLogger<MediaTransform> lLog)
        {
            log = lLog;
            _mediaServices = mediaServices;
            var t = MediaServicesClientHelper.GetAzureMediaServicesClientAsync(_mediaServices);
            t.Wait();
            _azureMediaServicesClient = (AzureMediaServicesClient)t.Result;
        }
        internal async Task<bool> TransformAsync(MediaTransformContext mtc)
        {

            // Ensure that you have the desired encoding Transform.
            Transform encodingTransform = await GetOrCreateTransformAsync(
                _azureMediaServicesClient,
                _mediaServices.mediaRg,
                _mediaServices.mediaMdsName,
                mtc.transformName);

            // Get the target files that need encoding:

            try
            {
                // Prepare unique strings to be used in the job

                string datetimeOA = DateTime.UtcNow.ToOADate().ToString().Replace(".", "-");
                string inputAssetName = "i-" + mtc.containerName;
                var inputAsset = await GetOrCreateInputAsset(inputAssetName, mtc.containerName);

                string outputAssetName = string.Format($"o-{mtc.containerName}-{datetimeOA}").ToLower();
                string outputAssetContainer = outputAssetName;

                // Create an ouput asset:
                var newOutputAsset = await _azureMediaServicesClient.Assets.CreateOrUpdateAsync(
                    _mediaServices.mediaRg,
                    _mediaServices.mediaMdsName,
                    outputAssetName,
                    new Asset()
                    {
                        Container = outputAssetContainer,
                        Description = outputAssetName,
                        StorageAccountName = _mediaServices.mediaStName,
                    });

                string jobName = $"job-{outputAssetName}";
                var jobInput = new JobInputAsset(inputAsset.Name);
                JobOutput[] jobOutputs = { new JobOutputAsset(outputAssetName) };

                // Submit the job:
                Job job = await _azureMediaServicesClient.Jobs.CreateAsync(
                    resourceGroupName: _mediaServices.mediaRg,
                    accountName: _mediaServices.mediaMdsName,
                    transformName: encodingTransform.Name,
                    jobName: jobName,
                    parameters: new Job
                    {
                        Input = jobInput,
                        Outputs = jobOutputs,
                        CorrelationData = new Dictionary<string, string>()
                        {
                            {"transformName", mtc.transformName },
                            {"datetimeOA", datetimeOA },
                            {"outputAssetContainer", outputAssetContainer },
                            {"transformedAssetName", outputAssetName },
                            {"mezzId", mtc.MezzId.ToString() },
                        },
                    });

                mtc.inputAssetName = inputAssetName;
                mtc.jobName = job.Name;
                mtc.jobId = job.Id;
                mtc.transformedAssetName = outputAssetName;
                mtc.outputAssetContainer = outputAssetContainer;

                log?.LogInformationObject(LogEventIds.MediaJobSubmittedSuccess, mtc);
            }
            catch (Microsoft.Azure.Management.Media.Models.ApiErrorException apiError)
            {
                mtc.Errors = apiError.Response.Content;
                log?.LogCriticalObject(LogEventIds.MediaAMSApiError, apiError, mtc);
                return false;
            }
            catch (Exception e)
            {
                log?.LogCriticalObject(LogEventIds.MediaUnknownException, e, mtc);
                return false;
            }
            return true;
        }

        private async Task<Asset> GetOrCreateInputAsset(string inputAssetName, string containerName)
        {
            var inputAsset = await _azureMediaServicesClient.Assets.GetAsync(
                _mediaServices.mediaRg,
                _mediaServices.mediaMdsName,
                inputAssetName);

            if (inputAsset == null)
            {
                // Create an input asset from the existing blob container location:
                inputAsset = await _azureMediaServicesClient.Assets.CreateOrUpdateAsync(
                    _mediaServices.mediaRg,
                    _mediaServices.mediaMdsName,
                    inputAssetName,
                    new Asset()
                    {
                        Container = containerName,
                        Description = inputAssetName,
                        StorageAccountName = _mediaServices.uploadStName,
                    });
            }

            return inputAsset;
        }

        private static async Task<Transform> GetOrCreateTransformAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string transformName)
        {
            Transform transform = await client.Transforms.GetAsync(resourceGroupName, accountName, transformName);

            if (transform == null)
            {

                //         AudioAnalyzerPreset aap = new AudioAnalyzerPreset(audioLanguage: "en-US");

                Preset p = new BuiltInStandardEncoderPreset()
                { PresetName = EncoderNamedPreset.AdaptiveStreaming };
                TransformOutput[] outputs = new TransformOutput[]
                {
                    //                    new TransformOutput(aap),
                    new TransformOutput(p),
                };
                transform = await client.Transforms.CreateOrUpdateAsync(
                    resourceGroupName,
                    accountName,
                    transformName,
                    outputs);

            }

            return transform;
        }


    }
}
