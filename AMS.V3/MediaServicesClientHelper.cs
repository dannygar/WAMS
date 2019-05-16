using System.Threading.Tasks;
using Microsoft.Azure.Management.Media;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest.Azure.Authentication;

namespace AMS.V3
{
    public class MediaServicesClientHelper
    {
        /// <summary>
        /// Returns an Azure Media Services client object for use with AMS API v3.
        ///
        /// Ref:
        ///         https://github.com/Azure-Samples/media-services-v3-dotnet-core-tutorials
        /// .
        /// </summary>
        /// <returns>The AzureMediaServicesClient object.</returns>
        public static async Task<IAzureMediaServicesClient> GetAzureMediaServicesClientAsync(MediaServicesCredentials mediaServicesSettings)
        {
            var clientCredential = new ClientCredential(mediaServicesSettings.AmsClientId, mediaServicesSettings.AmsClientSecret);
            var serviceClientCredentials = await ApplicationTokenProvider.LoginSilentAsync(mediaServicesSettings.AmsAadTenantDomain, 
                                                clientCredential, ActiveDirectoryServiceSettings.Azure);
            return new AzureMediaServicesClient(mediaServicesSettings.AmsRestApiEndpoint, serviceClientCredentials)
            {
                SubscriptionId = mediaServicesSettings.SubscriptionId,
            };
        }
    }
}
