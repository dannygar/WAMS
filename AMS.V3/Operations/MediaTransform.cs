using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AMS.V3.Models;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Logging;

namespace AMS.V3.Operations
{
    internal class MediaTransform
    {

        private MediaServicesCredentials _mediaServices { get; set; }
        private AzureMediaServicesClient _azureMediaServicesClient { get; set; }

        private readonly ILogger log = null;
        public MediaTransform(MediaServicesCredentials mediaServices, ILogger lLog)
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
                _mediaServices.ResourceGroup,
                _mediaServices.MediaAccountName,
                mtc.TransformName,
                mtc.Presets.ToArray());

            // Get the target files that need encoding:

            try
            {
                // Prepare unique strings to be used in the job

                string datetimeOA = DateTime.UtcNow.ToOADate().ToString().Replace(".", "-");
                string inputAssetName = "i-" + mtc.ContainerName;
                var inputAsset = await GetOrCreateInputAsset(inputAssetName, mtc.ContainerName);

                string outputAssetName = string.Format($"o-{mtc.ContainerName}-{datetimeOA}").ToLower();
                string outputAssetContainer = outputAssetName;

                // Create an ouput asset:
                var newOutputAsset = await _azureMediaServicesClient.Assets.CreateOrUpdateAsync(
                    _mediaServices.ResourceGroup,
                    _mediaServices.MediaAccountName,
                    outputAssetName,
                    new Asset()
                    {
                        Container = outputAssetContainer,
                        Description = outputAssetName,
                        StorageAccountName = _mediaServices.StorageAccountName,
                    });

                string jobName = $"job-{outputAssetName}";
                var jobInput = new JobInputAsset(inputAsset.Name);
                JobOutput[] jobOutputs = { new JobOutputAsset(outputAssetName) };

                // Submit the job:
                Job job = await _azureMediaServicesClient.Jobs.CreateAsync(
                    resourceGroupName: _mediaServices.ResourceGroup,
                    accountName: _mediaServices.MediaAccountName,
                    transformName: encodingTransform.Name,
                    jobName: jobName,
                    parameters: new Job
                    {
                        Input = jobInput,
                        Outputs = jobOutputs,
                        CorrelationData = new Dictionary<string, string>()
                        {
                            {"transformName", mtc.TransformName },
                            {"datetimeOA", datetimeOA },
                            {"outputAssetContainer", outputAssetContainer },
                            {"transformedAssetName", outputAssetName },
                            {"mezzId", mtc.MezzId.ToString() },
                        },
                    });

                mtc.InputAssetName = inputAssetName;
                mtc.JobName = job.Name;
                mtc.JobId = job.Id;
                mtc.TransformedAssetName = outputAssetName;
                mtc.OutputAssetContainer = outputAssetContainer;

                log?.LogInformation($"Success: {mtc.TransformedAssetName}");
            }
            catch (Microsoft.Azure.Management.Media.Models.ApiErrorException apiError)
            {
                mtc.Errors = apiError.Response.Content;
                log?.LogCritical(mtc.Errors, apiError);
                return false;
            }
            catch (Exception e)
            {
                log?.LogCritical(e.Message);
                return false;
            }
            return true;
        }

        private async Task<Asset> GetOrCreateInputAsset(string inputAssetName, string containerName)
        {
            var inputAsset = await _azureMediaServicesClient.Assets.GetAsync(
                _mediaServices.ResourceGroup,
                _mediaServices.MediaAccountName,
                inputAssetName);

            if (inputAsset == null)
            {
                // Create an input asset from the existing blob container location:
                inputAsset = await _azureMediaServicesClient.Assets.CreateOrUpdateAsync(
                    _mediaServices.ResourceGroup,
                    _mediaServices.MediaAccountName,
                    inputAssetName,
                    new Asset()
                    {
                        Container = containerName,
                        Description = inputAssetName,
                        StorageAccountName = _mediaServices.StorageAccountName,
                    });
            }

            return inputAsset;
        }

        private static async Task<Transform> GetOrCreateTransformAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string transformName,
            Preset[] presets)
        {
            // Does a Transform already exist with the desired name? Assume that an existing Transform with the desired name
            // also uses the same recipe or Preset for processing content.
            Transform transform = await client.Transforms.GetAsync(resourceGroupName, accountName, transformName);

            if (transform == null)
            {

                //         AudioAnalyzerPreset aap = new AudioAnalyzerPreset(audioLanguage: "en-US");
                //Preset p = new BuiltInStandardEncoderPreset()
                //{ PresetName = EncoderNamedPreset.AdaptiveStreaming };
                //                    new TransformOutput(aap),

                var outputs = presets.Select(preset => new TransformOutput(preset)).ToList();

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
