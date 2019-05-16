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
    internal class MediaPublish
    {
        private MediaServicesCredentials _mediaServices { get; set; }
        private AzureMediaServicesClient _azureMediaServicesClient { get; set; }
        private ContentKeyPolicyHelper _contentKeyPolicyHelpers { get; set; }

        private readonly ILogger log = null;
        public MediaPublish(MediaServicesCredentials mediaServices,
            ContentKeyPolicyHelper contentKeyPolicyHelpers,
            ILogger lLog)
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
                        log?.LogCritical(ex, MediaConstants.DefaultDrmContentKeyPolicyName);
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
                    _mediaServices.ResourceGroup,
                    _mediaServices.MediaAccountName,
                    streamingLocatorName);

                // If a streaming locator already exists for the asset, delete it
                if (slCheck != null)
                {
                    await _azureMediaServicesClient.StreamingLocators.DeleteAsync(
                        _mediaServices.ResourceGroup,
                        _mediaServices.MediaAccountName,
                        streamingLocatorName);
                }

                // Create new streaming locator for asset
                StreamingLocator sl = await _azureMediaServicesClient.StreamingLocators.CreateAsync(
                    _mediaServices.ResourceGroup,
                    _mediaServices.MediaAccountName,
                    streamingLocatorName,
                    streamingLocatorsCreationParameters);

                // Get the list of streaming URLs from the asset's locators and add to MediaTransformContext
                // TODO: Refactor and create new MediaPublishContext object instead of piggybacking on MediaTransformContext
                mpc.streamingUrls = await GetStreamingUrlsAsync(_mediaServices, _azureMediaServicesClient, mpc.transformedAssetName);
            }
            catch (Microsoft.Azure.Management.Media.Models.ApiErrorException apiError)
            {
                mpc.Errors = apiError.Response.Content;
                log?.LogError(mpc.Errors, apiError);
                return false;
            }
            catch (Exception e)
            {
                log?.LogCritical(e.Message);
                return false;
            }
            return true;
        }

        internal static string DefaultStreamingEndpointHostName = string.Empty;

        public async Task<Dictionary<string, string>> GetStreamingUrlsAsync(MediaServicesCredentials mediaServicesSettings, IAzureMediaServicesClient azureMediaServicesClient, string assetName)
        {

            var streamingUrls = new Dictionary<string, string>();
            ListStreamingLocatorsResponse listStreamingLocatorsResponse = null;
            try
            {
                listStreamingLocatorsResponse = await azureMediaServicesClient.Assets.ListStreamingLocatorsAsync(
                                                        mediaServicesSettings.ResourceGroup,
                                                        mediaServicesSettings.MediaAccountName,
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
                                                mediaServicesSettings.ResourceGroup,
                                                mediaServicesSettings.MediaAccountName,
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
                                                        mediaServicesSettings.ResourceGroup,
                                                        mediaServicesSettings.MediaAccountName,
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
