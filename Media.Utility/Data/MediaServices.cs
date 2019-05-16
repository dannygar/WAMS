using System;

namespace Media.Utility.Data
{
    public class MediaServices
    {
#pragma warning disable IDE1006 // Naming Styles
        public string spnCmptClientId { get; set; }
        public string spnCmptClientSecret { get; set; }
        public string tenantId { get; set; }
        public string subscriptionId { get; set; }
        public string mediaRg { get; set; }
        public string mediaMdsName { get; set; }
        public string mediaStName { get; set; }
        public string uploadStName { get; set; }
        public string gphEvntgrdQuerytoken { get; set; }

        public Uri AzureResourceManagementEndpoint { get { return new Uri("https://management.azure.com/"); } }
        public string ida_audience { get; set; }
        public string ida_issuer { get; set; }
        public string ida_EntitledGroupObjectId { get; set; }
        public string sts_openid_configuration { get; set; }

        ///  FairPlay configuration information:
        /// <summary>
        ///  FairPlay Application Secret Key in HEX
        /// </summary>
        public string fairplay_ask_hex { get; set; }
        /// <summary>
        /// FairPlay X509 certificate
        /// </summary>
        public string fairplay_cert_base64_encoded_file { get; set; }
        /// <summary>
        /// FairPlay x509 password
        /// </summary>
        public string fairplay_cert_password { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    }
}
