using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Media.Utility.Models.DRM
{

    public class DRMSettings
    {
        /***************  used in both ContentKeyPolicy and StreamingPolicy *********************/
        /// <summary>
        /// Define EncryptionProtocol configurations for each encryption-scheme/streaming-protocol combination
        /// </summary>
        public EnabledEncryptionProtocols EnabledEncryptionProtocols { get; set; }
        //the following 3 inputs are optional. If not specified in DRMSettings, the internal defaults are used.

        public bool EnbleOfflineMode { get; set; }
        /*
        For PlayReady: in ContentKeyPolicy, ContentKeyPolicyPlayReadyLicense.LicenseType                             = drmSettings.EnbleOfflineMode? PlayReadyLicenseType.Persistent : PlayReadyLicenseType.Nonpersistent;
        For Widevine:  in ContentKeyPolicy, widevine_template.policy_overrides.can_persist                           = drmSettings.EnbleOfflineMode,
        For FairPlay:  in StreamingPolicy,  StreamingPolicy.CommonEncryptionCbcs.Drm.FairPlay.AllowPersistentLicense = drmSettings.EnbleOfflineMode,  (Only StreamingPolicyFairPlayConfiguration has property AllowPersistentLicense)
        
        What if you need "offline-allowed" for some clients and "online-only" for the rest?
        v3:
        Suppose you allow FPS offline mode for some iOS devices but online only for the rest:
        STS issues two different types JWTs: one contains "offline" claim and the other contains "online".
        You need to create two ContentKeyPolicyRestriction's: one requires "offline" claim while the other requires "online" claim.
        You need to create two ContentKeyPolicy's: each using a different ContentKeyPolicyRestriction.
        You need to create two StreamingPolicy's: one with StreamingPolicyFairPlayConfiguration.AllowPersistentLicense = true while the other false.
        You need to create two StreamingLocator's: one using ContentKeyPolicy & StreamingPolicy corresponding to "offline", the other using ContentKeyPolicy & StreamingPolicy corresponding to "online".
        If this is only for PlayReady and Widevine, you can use a single StreamingPolicy, but still need two separate StreamingLocators. But you do NOT need to duplicate asset for this.
        v2:
        For PlayReady and Widevine
        You can still use a single IContentKeyAuthorizationPolicy for an IAsset without the need to duplicate IAsset. You need to use 2 IContentKeyAuthorizationPolicyOption's:
        [1] IContentKeyAuthorizationPolicyOption 1: uses persistent license, and ContentKeyAuthorizationPolicyRestriction 1 which contains a claim such as “Persistent”;
        [2] IContentKeyAuthorizationPolicyOption 2: uses nonpersistent license, and ContentKeyAuthorizationPolicyRestriction 2 which contains a claim such as “Nonpersisten”;
        A single AssetDeliveryPolicy is used. Single streaming URL is enough.
        For FPS, AssetDeliveryPolicyConfigurationKey.AllowPersistentLicense is used in creating AssetDeliveryPolicy. This means two different AssetDeliveryPolicy's are used.
        You have to have two separate assets, each with a streaming URL - one for online-only and the other for offline-allowed
        */


        /******************* used in ContentKeyPolicy ************************/
        public string ContentKeyPolicyName { get; set; }
        public STSType STSType { get; set; }  //None: Open restriction

        /// <summary>
        /// Define PlayReady license configuration
        /// </summary>
        public ContentKeyPolicyPlayReadyLicense PlayReadyLicenseConfig { get; set; }
        /// <summary>
        /// Define Widevine licesne configuration
        /// </summary>
        public widevine_template WidevineLicenseConfig { get; set; }
        /// <summary>
        /// Define FairPlay license configuration
        /// </summary>
        public ContentKeyPolicyFairPlayConfiguration FairPlayLicenseConfig { get; set; }

        public string PlayReadyResponseCustomData { get; set; }
        public ContentKeyPolicyTokenClaim[] TokenClaims { get; set; }
        //If you add ContentKeyPolicyTokenClaim.ContentKeyIdentifierClaim, the corresponding claim key is urn:microsoft:azure:mediaservices:contentkeyidentifier for each content key to be added to the JWT issued by custom STS. AAD does not do this.
        //This claim requires that the value of the claim in the token must match the key identifier of the key being requested by the client. Adding this claim means that the token issued to the client authorizes access to the content key identifier listed in the token.
        //objContentKeyPolicyTokenRestriction.RequiredClaims.Add(ContentKeyPolicyTokenClaim.ContentKeyIdentifierClaim);

        //public string WidevineCencHeaderContentId { get; set; }
        /*
        Widevine does not have Custom Attribute, but does allow content_id. There is no field for customer specified data as in the PlayReady header.  Adding the content identifier is a small feature that mostly requires adding the API value and plumbing it through to the origin for dynamic encryption to put in the header.
        PSSH: Below is the Widevine PSSH format for content providers who wish to synthesis the PSSH rather than using the ones returned by the API.  The structure below is a protocol buffer (see https://developers.google.com/protocol-buffers/).  
        The process is: 
            1) Build the protocol buffer message below. 
            2) Serialize the message to bytes. 
            3) Base64 encode the bytes. 
  
        message WidevineCencHeader
        {
           enum Algorithm {     UNENCRYPTED = 0;     AESCTR = 1;   };   
           optional Algorithm algorithm = 1;   
           repeated bytes key_id = 2; 
           // Content provider name.
           optional string provider = 3; 
           // A content identifier, specified by content provider.
           optional bytes content_id = 4; 
           // Track type. Acceptable values are SD, HD and AUDIO. Used to differentiate content keys used by an asset.   
           optional string track_type = 5; 
           // The name of a registered policy to be used for this asset.   
           optional string policy = 6; 
           // Crypto period index, for media using key rotation.   
           optional uint32 crypto_period_index = 7; 
        } 
        */


        /***************  used in StreamingPolicy *********************/

        public string PlayReadyCustomAttributes { get; set; }
        /*
        custom attribute format: name1=value1;name2=value2; PlayReady protection header custom attribute: <CUSTOMATTRIBUTES><CONTENTID>MMVD8869AD6F4108E217F2EB4A59E5CC65CC:9slcxjTePStCXf+Fmtiylw==</CONTENTID><IIS_DRM_VERSION>8.0.1608.1002</IIS_DRM_VERSION></CUSTOMATTRIBUTES>
        http://download.microsoft.com/download/2/0/2/202E5BD8-36C6-4DB8-9178-12472F8B119E/PlayReady%20Header%20Object%204-15-2013.docx
        It is recommended that the size of this field should not exceed 1kilobyte (KB).
        */

        //supports 2 URL templates: {AssetAlternateId} and {ContentKeyId}. E.g.
        //http://use2-2.api.microsoftstream.come/videos/{AssetAlternateId}/ProtectionKey?kid={CotnentKeyId}
        //RPv3 no longer has BaseLicenseAcquisitionUrl since it can be covered as a special case of CustomLicenseAcquisitionUrlTemplate.
        public CustomLicenseAcquisitionUrlTemplate CustomLicenseAcquisitionUrlTemplate { get; set; }

        public string StreamingPolicyName { get; set; }

    }

}
