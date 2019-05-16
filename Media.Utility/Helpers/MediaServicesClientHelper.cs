using Media.Utility.Data;
using Microsoft.Azure.Management.Media;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest.Azure.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Media.Utility.Helpers
{
    internal class MediaServicesClientHelper
    {
        /// <summary>
        /// Returns an Azure Media Services client object for use with AMS API v3.
        ///
        /// Ref:
        ///         https://github.com/Azure-Samples/media-services-v3-dotnet-core-tutorials
        /// .
        /// </summary>
        /// <returns>The AzureMediaServicesClient object.</returns>
        public static async Task<IAzureMediaServicesClient> GetAzureMediaServicesClientAsync(MediaServices mediaServicesSettings)
        {
            var clientCredential = new ClientCredential(mediaServicesSettings.spnCmptClientId, mediaServicesSettings.spnCmptClientSecret);
            var serviceClientCredentials = await ApplicationTokenProvider.LoginSilentAsync(mediaServicesSettings.tenantId, clientCredential, ActiveDirectoryServiceSettings.Azure);
            return new AzureMediaServicesClient(mediaServicesSettings.AzureResourceManagementEndpoint, serviceClientCredentials)
            {
                SubscriptionId = mediaServicesSettings.subscriptionId,
            };
        }
    }
}
