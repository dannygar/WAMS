using FilmDove.Logger;
using Media.Utility.Data;
using Media.Utility.Helpers;
using Media.Utility.Models;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Media.Utility.Operations
{
    class MediaPublish
    {
        private MediaServices _mediaServices { get; set; }
        private AzureMediaServicesClient _azureMediaServicesClient { get; set; }
        private ContentKeyPolicyHelper _contentKeyPolicyHelpers { get; set; }

        private readonly IGraphLogger<MediaPublish> log = null;
        public MediaPublish(MediaServices mediaServices,
            ContentKeyPolicyHelper contentKeyPolicyHelpers,
            IGraphLogger<MediaPublish> lLog)
        {
            log = lLog;
            _mediaServices = mediaServices;
            _contentKeyPolicyHelpers = contentKeyPolicyHelpers;
            var t = MediaServicesClientHelper.GetAzureMediaServicesClientAsync(_mediaServices);
            t.Wait();
            _azureMediaServicesClient = (AzureMediaServicesClient)t.Result;
        }
        public async Task<bool> PublishAsync(MediaPublishContext mpc)
        {
            string streamingLocatorName = $"Locator-{mpc.transformedAssetName}";

            try
            {
                StreamingLocator streamingLocatorsCreationParameters;
                // If we need to apply DRM, then we need a ContentKeyPolicy:
                if (mpc.streamingPolicy == PredefinedStreamingPolicy.MultiDrmCencStreaming ||
                    mpc.streamingPolicy == PredefinedStreamingPolicy.MultiDrmStreaming)
                {
                    if (_contentKeyPolicyHelpers.GetContentKeyPolicyByName(MediaConstants.DefaultDrmContentKeyPolicyName) == null)
                    {
                        var ex = new System.InvalidOperationException("Failed to create DRM content key policy.");
                        log?.LogCritical(LogEventIds.MediaDRMPolicyCreationError, ex, MediaConstants.DefaultDrmContentKeyPolicyName);
                        throw ex;
                    }

                    streamingLocatorsCreationParameters = new StreamingLocator(
                            mpc.transformedAssetName,
                            mpc.streamingPolicy)
                    { DefaultContentKeyPolicyName = MediaConstants.DefaultDrmContentKeyPolicyName };
                }
                else
                {
                    streamingLocatorsCreationParameters = new StreamingLocator(
                        mpc.transformedAssetName,
                        mpc.streamingPolicy);
                }

                // Check to see if streaming locator already exists for the asset
                StreamingLocator slCheck = await _azureMediaServicesClient.StreamingLocators.GetAsync(
                    _mediaServices.mediaRg,
                    _mediaServices.mediaMdsName,
                    streamingLocatorName);

                // If a streaming locator already exists for the asset, delete it
                if (slCheck != null)
                {
                    await _azureMediaServicesClient.StreamingLocators.DeleteAsync(
                        _mediaServices.mediaRg,
                        _mediaServices.mediaMdsName,
                        streamingLocatorName);
                }

                // Create new streaming locator for asset
                StreamingLocator sl = await _azureMediaServicesClient.StreamingLocators.CreateAsync(
                    _mediaServices.mediaRg,
                    _mediaServices.mediaMdsName,
                    streamingLocatorName,
                    streamingLocatorsCreationParameters);

                // Get the list of streaming URLs from the asset's locators and add to MediaTransformContext
                // TODO: Refactor and create new MediaPublishContext object instead of piggybacking on MediaTransformContext
                mpc.streamingUrls = await GetStreamingUrlsAsync(_mediaServices, _azureMediaServicesClient, mpc.transformedAssetName);
                log?.LogInformationObject(LogEventIds.MediaLocatorPublished, mpc);
            }
            catch (Microsoft.Azure.Management.Media.Models.ApiErrorException apiError)
            {
                mpc.Errors = apiError.Response.Content;
                log?.LogCriticalObject(LogEventIds.MediaAMSApiError, apiError, mpc);
                return false;
            }
            catch (Exception e)
            {
                log?.LogCriticalObject(LogEventIds.MediaUnknownException, e, mpc);
                return false;
            }
            return true;
        }

        internal static string DefaultStreamingEndpointHostName = string.Empty;

        public async Task<Dictionary<string, string>> GetStreamingUrlsAsync(MediaServices mediaServicesSettings, IAzureMediaServicesClient azureMediaServicesClient, string assetName)
        {

            var streamingUrls = new Dictionary<string, string>();
            ListStreamingLocatorsResponse listStreamingLocatorsResponse = null;
            try
            {
                listStreamingLocatorsResponse = await azureMediaServicesClient.Assets.ListStreamingLocatorsAsync(
                                                        mediaServicesSettings.mediaRg,
                                                        mediaServicesSettings.mediaMdsName,
                                                        assetName);
            }
            catch (Exception e)
            {
                streamingUrls.Add("error", $".Assets.ListStreamingLocatorsAsync assetName {assetName}");
                //TODO: Log 
            }

            string locatorName = listStreamingLocatorsResponse?.StreamingLocators?.FirstOrDefault()?.Name;
            if (!string.IsNullOrEmpty(locatorName))
            {

                ListPathsResponse listPathsResponse = null;
                try
                {

                    listPathsResponse = await azureMediaServicesClient.StreamingLocators.ListPathsAsync(
                                                mediaServicesSettings.mediaRg,
                                                mediaServicesSettings.mediaMdsName,
                                                locatorName);
                }
                catch (Exception e)
                {
                    streamingUrls.Add("error", $".StreamingLocators.ListPathsAsync locatorName {locatorName}");
                    //TODO: Log 
                }

                if (listPathsResponse?.StreamingPaths?.Count() > 0)
                {
                    if (string.IsNullOrEmpty(DefaultStreamingEndpointHostName))
                    {
                        const string DefaultStreamingEndpointName = "default";
                        StreamingEndpoint streamingEndpoint = null;
                        try
                        {
                            streamingEndpoint = await azureMediaServicesClient.StreamingEndpoints.GetAsync(
                                                        mediaServicesSettings.mediaRg,
                                                        mediaServicesSettings.mediaMdsName,
                                                        DefaultStreamingEndpointName);
                        }
                        catch (Exception e)
                        {
                            streamingUrls.Add("error", $".StreamingEndpoints.GetAsync DefaultStreamingEndpointName {DefaultStreamingEndpointName}");
                            //TODO: Log 
                        }

                        if (streamingEndpoint != null)
                        {
                            DefaultStreamingEndpointHostName = streamingEndpoint.HostName;
                        }
                    }

                    if (!string.IsNullOrEmpty(DefaultStreamingEndpointHostName))
                    {
                        foreach (StreamingPath path in listPathsResponse.StreamingPaths)
                        {
                            UriBuilder uriBuilder = new UriBuilder();
                            uriBuilder.Scheme = "https";
                            uriBuilder.Host = DefaultStreamingEndpointHostName;
                            uriBuilder.Path = path.Paths.FirstOrDefault();
                            streamingUrls.Add(path.StreamingProtocol, uriBuilder.ToString());
                        }
                    }
                }
            }

            return streamingUrls;
        }

    }
}
