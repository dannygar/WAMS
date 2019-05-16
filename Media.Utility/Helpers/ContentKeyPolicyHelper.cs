using FilmDove.Logger;
using Media.Utility.Data;
using Media.Utility.Models.DRM;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Media.Utility.Helpers
{
    internal class ContentKeyPolicyHelper
    {

        private MediaServices _mediaServices { get; set; }
        private AzureMediaServicesClient _azureMediaServicesClient { get; set; }
        private readonly IGraphLogger<ContentKeyPolicyHelper> _log;

        public ContentKeyPolicyHelper(MediaServices mediaServices,
                                      IGraphLogger<ContentKeyPolicyHelper> log)
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
                _azureMediaServicesClient.ContentKeyPolicies.Delete(_mediaServices.mediaRg, _mediaServices.mediaMdsName, contentKeyPolicyName);
            }
            catch
            {
                _log?.LogInformationObject(LogEventIds.MediaDrmGeneralInformation, $"ContentKeyPolicy {contentKeyPolicyName} was not deleted.");
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
                contentKeyPolicy = _azureMediaServicesClient.ContentKeyPolicies.GetAsync(_mediaServices.mediaRg, _mediaServices.mediaMdsName, contentKeyPolicyName).Result;
                if (contentKeyPolicy != null)
                {
                    _log?.LogInformationObject(LogEventIds.MediaDrmContentKeyPolicyExists, $"ContentKeyPolicy: {contentKeyPolicyName}");
                    return contentKeyPolicy;
                }

                // TODO: Move this into the calling app.
                // This configures what kind of encryption will be done.
                DRMSettings drmSettings = GetDrmSettings(contentKeyPolicyName);

                // First creation policy restriction object:
                ContentKeyPolicyRestriction contentKeyPolicyRestriction = CreateContentKeyPolicyRestriction(drmSettings);

                // Now create the ContentKeyPolicy with it:
                contentKeyPolicy = CreateContentKeyPolicy(_azureMediaServicesClient, _mediaServices.mediaRg, _mediaServices.mediaMdsName, drmSettings, contentKeyPolicyRestriction);

            }
            catch (Microsoft.Azure.Management.Media.Models.ApiErrorException apiError)
            {
                _log?.LogCriticalObject(LogEventIds.MediaDrmContentKeyPolicyOptionApiErrorException, apiError, apiError.Response?.Content);
                return null;
            }
            catch (Exception e)
            {
                _log?.LogCriticalObject(LogEventIds.MediaDrmContentKeyPolicyOptionException, e);
                return null;
            }

            return contentKeyPolicy;
        }

        private DRMSettings GetDrmSettings(string contentKeyPolicyName)
        {
            var drmSettings = new DRMSettings()
            {
                EnabledEncryptionProtocols = new EnabledEncryptionProtocols()
                {
                    //PlayReady, Widevine
                    CENC = new EncryptionProtocol()
                    {
                        EncryptionEnabled = true,
                    },
                    //FairPlay
                    CBCS = new EncryptionProtocol()
                    {
                        EncryptionEnabled = false,
                    },
                    //AES-128 clear key
                    CBC = new EncryptionProtocol()
                    {
                        // This enables encryption without secure key delivery.  
                        // FilmDove Note: If you opt for this, change: 
                        //      [Web]\src\app\azure-storage\mezzanine-utility.service.ts
                        // To: 
                        //      MediaEncryptionFormats { AAPL = '(format=m3u8-aapl)',  CENC = '(format=mpd-time-csf,encryption=cenc)'
                        // There is no need to set an encryption format for clear-key delivery of CBC encrypted HLS.
                        // Ref: https://docs.microsoft.com/en-us/azure/media-services/previous/media-services-protect-with-aes128
                        EncryptionEnabled = false,
                    },
                },
                STSType = STSType.AsymmetricWithKeyDiscovery,
                PlayReadyResponseCustomData = null,
                PlayReadyCustomAttributes = string.Empty,
                EnbleOfflineMode = true,
                TokenClaims = new ContentKeyPolicyTokenClaim[] { new ContentKeyPolicyTokenClaim("groups", _mediaServices.ida_EntitledGroupObjectId) },
                ContentKeyPolicyName = contentKeyPolicyName,
            };

            return drmSettings;
        }

        private ContentKeyPolicy CreateContentKeyPolicy(AzureMediaServicesClient client, string resourceGroup, string accountName, DRMSettings drmSettings, ContentKeyPolicyRestriction objContentKeyPolicyRestriction)
        {
            List<ContentKeyPolicyOption> objList_ContentKeyPolicyOption = new List<ContentKeyPolicyOption>();

            // Prepare the collection of Content-Key Policy-Options:
            try
            {
                //CENC
                ContentKeyPolicyOption contentKeyPolicyOptionCencPlayReady = GetContentKeyPolicyOptionCencPlayReady(drmSettings, objContentKeyPolicyRestriction);
                if (contentKeyPolicyOptionCencPlayReady != null)
                {
                    objList_ContentKeyPolicyOption.Add(contentKeyPolicyOptionCencPlayReady);
                }
                ContentKeyPolicyOption contentKeyPolicyOptionCencWidevine = GetContentKeyPolicyOptionCencWidevine(drmSettings, objContentKeyPolicyRestriction);
                if (contentKeyPolicyOptionCencWidevine != null)
                {
                    objList_ContentKeyPolicyOption.Add(contentKeyPolicyOptionCencWidevine);
                }

                //CBCS
                ContentKeyPolicyOption contentKeyPolicyOptionCbcs = GetContentKeyPolicyOptionsCbcs(drmSettings, objContentKeyPolicyRestriction);
                if (contentKeyPolicyOptionCbcs != null)
                {
                    objList_ContentKeyPolicyOption.Add(contentKeyPolicyOptionCbcs);
                }

                //CBC
                ContentKeyPolicyOption contentKeyPolicyOptionCbc = GetContentKeyPolicyOptionCbc(drmSettings, objContentKeyPolicyRestriction);
                if (contentKeyPolicyOptionCbc != null)
                {
                    objList_ContentKeyPolicyOption.Add(contentKeyPolicyOptionCbc);
                }

            }
            catch (Microsoft.Azure.Management.Media.Models.ApiErrorException apiError)
            {
                _log?.LogCriticalObject(LogEventIds.MediaDrmContentKeyPolicyOptionApiErrorException, apiError, apiError.Response?.Content);
                return null;
            }
            catch (Exception e)
            {
                _log?.LogCriticalObject(LogEventIds.MediaDrmContentKeyPolicyOptionException, e);
                return null;
            }

            // Create the ContentKey:
            ContentKeyPolicy contentKeyPolicy;
            try
            {
                contentKeyPolicy = client.ContentKeyPolicies.CreateOrUpdate(
                    resourceGroup,
                    accountName,
                    drmSettings.ContentKeyPolicyName,
                    objList_ContentKeyPolicyOption
                    );
            }
            catch (Microsoft.Azure.Management.Media.Models.ApiErrorException apiError)
            {
                _log?.LogCriticalObject(LogEventIds.MediaDrmContentKeyPolicyApiErrorException, apiError, apiError.Response?.Content);
                return null;
            }
            catch (Exception e)
            {
                _log?.LogCriticalObject(LogEventIds.MediaDrmContentKeyPolicyException, e);
                return null;
            }

            _log?.LogInformationObject(LogEventIds.MediaDrmContentKeyPolicyCreated, $"ContentKeyPolicy {drmSettings.ContentKeyPolicyName}");

            return contentKeyPolicy;
        }


        private ContentKeyPolicyOption GetContentKeyPolicyOptionCencWidevine(DRMSettings drmSettings, ContentKeyPolicyRestriction objContentKeyPolicyRestriction)
        {
            ContentKeyPolicyOption contentKeyPolicyOptionCencWidevine = null;
            if (drmSettings.EnabledEncryptionProtocols.CENC.EncryptionEnabled)
            {
                //Widevine      
                widevine_template objwidevine_template;
                if (drmSettings.WidevineLicenseConfig == null)
                {
                    objwidevine_template = new widevine_template()
                    {
                        allowed_track_types = "SD_HD",
                        content_key_specs = new content_key_spec[]
                            {
                                new content_key_spec()
                                {
                                    track_type = "SD",
                                    security_level = 1,
                                    required_output_protection = new output_protection()
                                    {
                                    hdcp = "HDCP_NONE"
                                    }
                                }
                            },
                        policy_overrides = new policy_overrides()
                        {
                            can_play = true,
                            can_persist = drmSettings.EnbleOfflineMode,
                            can_renew = false,
                            license_duration_seconds = 2592000,
                            playback_duration_seconds = 10800,
                            rental_duration_seconds = 604800,
                        }
                    };
                }
                else
                {
                    objwidevine_template = drmSettings.WidevineLicenseConfig;
                    _log?.LogInformationObject(LogEventIds.MediaDrmGeneralInformation, $"WidevineLicenseConfig: input specification is used.");
                }
                ContentKeyPolicyWidevineConfiguration objContentKeyPolicyWidevineConfiguration = new ContentKeyPolicyWidevineConfiguration
                {
                    WidevineTemplate = Newtonsoft.Json.JsonConvert.SerializeObject(objwidevine_template)
                };
                contentKeyPolicyOptionCencWidevine =
                    new ContentKeyPolicyOption()
                    {
                        Configuration = objContentKeyPolicyWidevineConfiguration,
                        Restriction = objContentKeyPolicyRestriction,
                        Name = "ContentKeyPolicyOption_CENC_Widevine"
                    };
            }

            return contentKeyPolicyOptionCencWidevine;
        }

        private ContentKeyPolicyOption GetContentKeyPolicyOptionCencPlayReady(DRMSettings drmSettings, ContentKeyPolicyRestriction objContentKeyPolicyRestriction)
        {
            ContentKeyPolicyOption contentKeyPolicyOptionCencPlayReady = null;
            if (drmSettings.EnabledEncryptionProtocols.CBCS.EncryptionEnabled)
            {
                //PlayReady
                ContentKeyPolicyPlayReadyLicense objContentKeyPolicyPlayReadyLicense;
                if (drmSettings.PlayReadyLicenseConfig == null)
                {
                    objContentKeyPolicyPlayReadyLicense = new ContentKeyPolicyPlayReadyLicense
                    {
                        AllowTestDevices = true,
                        BeginDate = new DateTime(2016, 1, 1),
                        ContentKeyLocation = new ContentKeyPolicyPlayReadyContentEncryptionKeyFromHeader(),
                        ContentType = ContentKeyPolicyPlayReadyContentType.UltraVioletStreaming,
                        LicenseType = drmSettings.EnbleOfflineMode ? ContentKeyPolicyPlayReadyLicenseType.Persistent : ContentKeyPolicyPlayReadyLicenseType.NonPersistent,
                        PlayRight = new ContentKeyPolicyPlayReadyPlayRight
                        {
                            ImageConstraintForAnalogComponentVideoRestriction = true,
                            ExplicitAnalogTelevisionOutputRestriction = new ContentKeyPolicyPlayReadyExplicitAnalogTelevisionRestriction(true, 2),
                            AllowPassingVideoContentToUnknownOutput = ContentKeyPolicyPlayReadyUnknownOutputPassingOption.Allowed,
                            FirstPlayExpiration = TimeSpan.FromSeconds(20.0),
                        }
                    };
                }
                else
                {
                    objContentKeyPolicyPlayReadyLicense = drmSettings.PlayReadyLicenseConfig;
                    _log?.LogInformationObject(LogEventIds.MediaDrmGeneralInformation, $"PlayReadyLicenseConfig: input specification is used.");
                }
                ContentKeyPolicyPlayReadyConfiguration objContentKeyPolicyPlayReadyConfiguration = new ContentKeyPolicyPlayReadyConfiguration
                {
                    ResponseCustomData = drmSettings.PlayReadyResponseCustomData,
                    Licenses = new List<ContentKeyPolicyPlayReadyLicense> { objContentKeyPolicyPlayReadyLicense },
                };
                contentKeyPolicyOptionCencPlayReady =
                    new ContentKeyPolicyOption()
                    {
                        Configuration = objContentKeyPolicyPlayReadyConfiguration,
                        Restriction = objContentKeyPolicyRestriction,
                        Name = "ContentKeyPolicyOption_CENC_PlayReady"
                    };
            }

            return contentKeyPolicyOptionCencPlayReady;
        }


        private ContentKeyPolicyOption GetContentKeyPolicyOptionsCbcs(DRMSettings drmSettings, ContentKeyPolicyRestriction objContentKeyPolicyRestriction)
        {
            ContentKeyPolicyOption contentKeyPolicyOptionCbcs = null;
            if (drmSettings.EnabledEncryptionProtocols.CBCS.EncryptionEnabled)
            {
                //FairPlay
                // Ref: https://docs.microsoft.com/en-us/azure/media-services/previous/media-services-protect-hls-with-fairplay

                string askHex = _mediaServices.fairplay_ask_hex;
                if (string.IsNullOrEmpty(_mediaServices.fairplay_ask_hex))
                {
                    string message = $"GetContentKeyPolicyOptionsCbcs: fairplay_ask_hex is null or empty.";
                    _log?.LogErrorObject(LogEventIds.MediaDrmConfigurationError, message);
                    throw new ArgumentNullException(nameof(_mediaServices.fairplay_ask_hex), message);
                }
                if (!Guid.TryParse(askHex, out Guid askGuid))
                {
                    string message = $"GetContentKeyPolicyOptionsCbcs: failed to parse ASK as a GUID.";
                    _log?.LogErrorObject(LogEventIds.MediaDrmConfigurationError, message);
                    throw new ArgumentException(message, nameof(_mediaServices.fairplay_ask_hex));
                }
                byte[] askBytes = askGuid.ToByteArray();

                if (string.IsNullOrEmpty(_mediaServices.fairplay_cert_base64_encoded_file))
                {
                    string message = $"GetContentKeyPolicyOptionsCbcs: fairplay_cert_base64_encoded_file is null or empty.";
                    _log?.LogErrorObject(LogEventIds.MediaDrmConfigurationError, message);
                    throw new ArgumentNullException(nameof(_mediaServices.fairplay_cert_base64_encoded_file), message);
                }

                if (string.IsNullOrEmpty(_mediaServices.fairplay_cert_password))
                {
                    string message = $"GetContentKeyPolicyOptionsCbcs: fairplay_cert_password is null or empty.";
                    _log?.LogErrorObject(LogEventIds.MediaDrmConfigurationError, message);
                    throw new ArgumentNullException(nameof(_mediaServices.fairplay_cert_password), message);
                }

                string fairPlayPfx;
                try
                {
                    fairPlayPfx = GetBase64EncodedCertFileAsPfxBase64EncodedCertString(
                                    _mediaServices.fairplay_cert_base64_encoded_file,
                                    _mediaServices.fairplay_cert_password);
                }
                catch (Exception e)
                {
                    string message = $"Failed while trying to read and convert {nameof(_mediaServices.fairplay_cert_base64_encoded_file)}: {_mediaServices.fairplay_cert_base64_encoded_file}";
                    _log?.LogErrorObject(LogEventIds.MediaDrmConfigurationError, message);
                    throw new Exception(message, e);
                }

                ContentKeyPolicyFairPlayConfiguration objContentKeyPolicyFairPlayConfiguration = new ContentKeyPolicyFairPlayConfiguration
                {
                    Ask = askBytes,             // FairPlay: Application Secret Key
                    FairPlayPfx = fairPlayPfx,  // FairPlayPfx must be a valid Base64 encoded PKCS#12 certificate with private key exported to PFX format with FairPlayPfxPassword.
                    FairPlayPfxPassword = _mediaServices.fairplay_cert_password,
                    RentalAndLeaseKeyType = drmSettings.FairPlayLicenseConfig == null ? ContentKeyPolicyFairPlayRentalAndLeaseKeyType.PersistentUnlimited : drmSettings.FairPlayLicenseConfig.RentalAndLeaseKeyType,   //this does not enable offline mode. Offline mode is enabled in StreamingPolicy.
                    RentalDuration = drmSettings.FairPlayLicenseConfig == null ? 2249 : drmSettings.FairPlayLicenseConfig.RentalDuration,
                };
                if (drmSettings.FairPlayLicenseConfig != null)
                {
                    _log?.LogInformationObject(LogEventIds.MediaDrmGeneralInformation, $"FairPlayLicenseConfig: input specification is used.");
                }
                contentKeyPolicyOptionCbcs =
                    new ContentKeyPolicyOption()
                    {
                        Configuration = objContentKeyPolicyFairPlayConfiguration,
                        Restriction = objContentKeyPolicyRestriction,
                        Name = "ContentKeyPolicyOption_CBCS"
                    };
            }

            return contentKeyPolicyOptionCbcs;
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

        private static ContentKeyPolicyOption GetContentKeyPolicyOptionCbc(DRMSettings drmSettings, ContentKeyPolicyRestriction objContentKeyPolicyRestriction)
        {
            // Ref: https://docs.microsoft.com/en-us/azure/media-services/previous/media-services-protect-with-aes128
            ContentKeyPolicyOption contentKeyPolicyOptionCbc = null;
            if (drmSettings.EnabledEncryptionProtocols.CBC.EncryptionEnabled)
            {
                contentKeyPolicyOptionCbc =
                    new ContentKeyPolicyOption()
                    {
                        Configuration = new ContentKeyPolicyClearKeyConfiguration(),
                        Restriction = objContentKeyPolicyRestriction,
                        Name = "ContentKeyPolicyOption_CBC"
                    };
            }

            return contentKeyPolicyOptionCbc;
        }

        private ContentKeyPolicyRestriction CreateContentKeyPolicyRestriction(DRMSettings drmSettings)
        {
            ContentKeyPolicyRestriction objContentKeyPolicyRestriction;
            switch (drmSettings.STSType)
            {
                case STSType.AsymmetricWithKeyDiscovery:
                    if (string.IsNullOrEmpty(_mediaServices.sts_openid_configuration))
                    {
                        string message = $"CreateContentKeyPolicyRestriction: sts_openid_configuration is null or empty.";
                        _log?.LogErrorObject(LogEventIds.MediaDrmConfigurationError, message);
                        throw new ArgumentNullException(nameof(_mediaServices.sts_openid_configuration), message);
                    }
                    objContentKeyPolicyRestriction = new ContentKeyPolicyTokenRestriction()
                    {
                        OpenIdConnectDiscoveryDocument = _mediaServices.sts_openid_configuration,
                    };
                    break;
                case STSType.AAD:        //use Azure Active Directory OpenId discovery document, supporting key rollover
                case STSType.Symmetric:  //use symmetric key in custom STS
                case STSType.Asymmetric: //use asymmetric key/X509 cert in custom STS
                default:
                    throw new NotSupportedException($"No support for {drmSettings.STSType}");
            }

            //add claims if ContentkeyPolicyTokenRestriction
            if (drmSettings.STSType != STSType.None)
            {
                if (string.IsNullOrEmpty(_mediaServices.ida_audience))
                {
                    string message = $"CreateContentKeyPolicyRestriction: ida_audience is null or empty.";
                    _log?.LogErrorObject(LogEventIds.MediaDrmConfigurationError, message);
                    throw new ArgumentNullException(nameof(_mediaServices.ida_audience), message);
                }
                if (string.IsNullOrEmpty(_mediaServices.ida_issuer))
                {
                    string message = $"CreateContentKeyPolicyRestriction: ida_issuer is null or empty.";
                    _log?.LogErrorObject(LogEventIds.MediaDrmConfigurationError, message);
                    throw new ArgumentNullException(nameof(_mediaServices.ida_issuer), message);
                }

                ContentKeyPolicyTokenRestriction objContentKeyPolicyTokenRestriction = (ContentKeyPolicyTokenRestriction)objContentKeyPolicyRestriction;
                objContentKeyPolicyTokenRestriction.Audience = _mediaServices.ida_audience;
                objContentKeyPolicyTokenRestriction.Issuer = _mediaServices.ida_issuer;
                objContentKeyPolicyTokenRestriction.RestrictionTokenType = ContentKeyPolicyRestrictionTokenType.Jwt;

                if (drmSettings.TokenClaims != null && drmSettings.TokenClaims.Length > 0)
                {
                    objContentKeyPolicyTokenRestriction.RequiredClaims = new List<ContentKeyPolicyTokenClaim>(drmSettings.TokenClaims);
                }

                objContentKeyPolicyRestriction = objContentKeyPolicyTokenRestriction;
            }

            return objContentKeyPolicyRestriction;
        }
    }
}
