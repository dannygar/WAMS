using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Logging;

namespace AMS.V3
{
    public class ContentKeyPolicyHelper
    {

        private MediaServicesCredentials _mediaServices { get; set; }
        private AzureMediaServicesClient _azureMediaServicesClient { get; set; }
        private readonly ILogger _log;

        public ContentKeyPolicyHelper(MediaServicesCredentials mediaServices,
            ILogger log)
        {
            _log = log;
            _mediaServices = mediaServices;
            var t = MediaServicesClientHelper.GetAzureMediaServicesClientAsync(_mediaServices);
            t.Wait();
            _azureMediaServicesClient = (AzureMediaServicesClient)t.Result;
        }

        public bool DeleteContentKeyPolicyByName(string contentKeyPolicyName)
        {
            bool result = true;
            try
            {
                _azureMediaServicesClient.ContentKeyPolicies.Delete(_mediaServices.ResourceGroup, _mediaServices.MediaAccountName, contentKeyPolicyName);
            }
            catch
            {
                _log?.LogInformation($"ContentKeyPolicy {contentKeyPolicyName} was not deleted.");
                result = false;
            }
            return result;
        }

        public ContentKeyPolicy GetContentKeyPolicyByName(string contentKeyPolicyName)
        {
            // Get, if exists:
            ContentKeyPolicy contentKeyPolicy;
            try
            {
                contentKeyPolicy = _azureMediaServicesClient.ContentKeyPolicies.GetAsync(_mediaServices.ResourceGroup, 
                    _mediaServices.MediaAccountName, contentKeyPolicyName).Result;
                if (contentKeyPolicy != null)
                {
                    _log?.LogInformation($"ContentKeyPolicy: {contentKeyPolicyName}");
                }
            }
            catch (Microsoft.Azure.Management.Media.Models.ApiErrorException apiError)
            {
                _log?.LogError(apiError, apiError.Response?.Content);
                return null;
            }
            catch (Exception e)
            {
                _log?.LogError(e.Message);
                return null;
            }

            return contentKeyPolicy;
        }

        /// <summary>
        /// Reads Base64 Encoded Cert File, converts to Pfx Base64 Encoded Cert String
        /// </summary>
        /// <param name="fairplayCertBase64EncodedFilePath">File Path</param>
        /// <param name="fairplayCertPassword">Cert password</param>
        /// <returns></returns>
        private string GetBase64EncodedCertFileAsPfxBase64EncodedCertString(string fairplayCertBase64EncodedFilePath, string fairplayCertPassword)
        {
            var x509Base64RawData = File.ReadAllText(fairplayCertBase64EncodedFilePath);
            byte[] x509RawData = Convert.FromBase64String(x509Base64RawData);
            X509Certificate2 objX509Certificate2 = new X509Certificate2(x509RawData, fairplayCertPassword, X509KeyStorageFlags.Exportable);
            return Convert.ToBase64String(objX509Certificate2.Export(X509ContentType.Pfx, fairplayCertPassword));
        }

    }
}
