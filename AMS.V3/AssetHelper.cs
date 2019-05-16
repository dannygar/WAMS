using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AMS.V3.Operations;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Newtonsoft.Json.Linq;

namespace AMS.V3
{
    internal class AssetHelper
    {

        private MediaServicesCredentials _mediaServices { get; set; }
        private AzureMediaServicesClient _azureMediaServicesClient { get; set; }

        private MediaPublish _mediaPublish { get; set; }
        public AssetHelper(MediaServicesCredentials mediaServices,
            MediaPublish mediaPublish)
        {

            _mediaServices = mediaServices ?? throw new ArgumentNullException(nameof(mediaServices));
            var t = MediaServicesClientHelper.GetAzureMediaServicesClientAsync(_mediaServices);
            t.Wait();
            _azureMediaServicesClient = (AzureMediaServicesClient)t.Result;
            _mediaPublish = mediaPublish ?? throw new ArgumentNullException(nameof(mediaPublish)); ;
        }

        public async Task<JArray> GetAllAssetNamesAsync()
        {
            List<Asset> assetList = await GetAllAssets();
            var assetNames = assetList.Select(a => a.Name);
            return JArray.FromObject(assetNames);
        }

        public async Task<List<Asset>> GetAllAssets(int maxPages = 10)
        {
            var assetList = new List<Asset>();
            string nextPageLink = string.Empty;
            Page<Asset> page = null;
            do
            {
                if (string.IsNullOrEmpty(nextPageLink))
                {
                    page = (Page<Asset>)await _azureMediaServicesClient.Assets.ListAsync(_mediaServices.ResourceGroup, _mediaServices.MediaAccountName);
                }
                else
                {
                    page = (Page<Asset>)await _azureMediaServicesClient.Assets.ListNextAsync(nextPageLink);
                }
                if (page != null)
                {
                    assetList.AddRange(page);
                    nextPageLink = page.NextPageLink;
                }
                maxPages--;
            } while (!string.IsNullOrEmpty(nextPageLink) && maxPages > 0);
            return assetList;
        }

        public async Task<Asset> GetAssetAsync(string assetId)
        {
            List<Asset> assetList = await GetAllAssets();
            return assetList.FirstOrDefault(a => a.Id == assetId);
        }


        public async Task<JObject> GetAllLocatorsAsync()
        {
            var assestAndTheirLocators = new Dictionary<string, Dictionary<string, string>>();
            List<Asset> assetList = await GetAllAssets();

            foreach (var asset in assetList)
            {
                var streamingLocators = await _mediaPublish.GetStreamingUrlsAsync(_mediaServices, _azureMediaServicesClient, asset.Name);
                assestAndTheirLocators.Add(asset.Name, streamingLocators);
            }
            return JObject.FromObject(assestAndTheirLocators);
        }
    }
}
