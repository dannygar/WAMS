using System;
using System.Threading.Tasks;
using AMS.V3.Models;
using AMS.V3.Operations;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace AMS.V3.Services
{
    public class MediaUtilityService : IMediaUtilityService, IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MediaPublish _mediaPublish;
        private readonly AssetHelper _assetHelper;
        private readonly MediaServicesCredentials _mediaServices;
        private readonly ContentKeyPolicyHelper _contentKeyPolicyHelper;
        private readonly JobHelper _jobHelper;
        private readonly MediaTransform _mediaTransform;


        public object GetService(Type serviceType)
        {
            switch (serviceType.Name)
            {
                case "MediaPublish":
                    return _mediaPublish;
                case "AssetHelper":
                    return _assetHelper;
                case "JobHelper":
                    return _jobHelper;
                case "MediaTransform":
                    return _mediaTransform;
                default:
                    return null;
            }
        }

        public MediaUtilityService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Registers the classes needed by the libary within the DependencyInjection collection of the calling app.
        /// </summary>
        /// <param name="serviceCollection"></param>
        public static void UseMediaUtilityService(IServiceCollection serviceCollection, MediaServicesCredentials mediaServices)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.AddSingleton<MediaServicesCredentials>(mediaServices);
            serviceCollection.AddTransient<ContentKeyPolicyHelper>();
            serviceCollection.AddTransient<AssetHelper>();
            serviceCollection.AddTransient<JobHelper>();
            serviceCollection.AddTransient<MediaTransform>();
            serviceCollection.AddTransient<MediaPublish>();
            serviceCollection.AddTransient<IMediaUtilityService, MediaUtilityService>();

        }


        public MediaUtilityService(MediaServicesCredentials mediaServices, ILogger logger)
        {
            _mediaServices = mediaServices;
            _contentKeyPolicyHelper = new ContentKeyPolicyHelper(mediaServices, logger);
            _mediaPublish = new MediaPublish(mediaServices, _contentKeyPolicyHelper, logger );
            _assetHelper = new AssetHelper(mediaServices, _mediaPublish);
            _jobHelper = new JobHelper(mediaServices, logger);
            _mediaTransform = new MediaTransform(mediaServices, logger);
            _serviceProvider = this;
        }



        public async Task<bool> TransformAsync(MediaTransformContext mtc)
        {
            var mediaTransform = _serviceProvider.GetService<MediaTransform>();
            return await mediaTransform.TransformAsync(mtc);

        }

        public async Task<bool> PublishAsync(MediaPublishContext mpc)
        {
            var mediaPublish = _serviceProvider.GetService<MediaPublish>();
            return await mediaPublish.PublishAsync(mpc);
        }

        public async Task<JArray> GetAllAssetNamesAsync()
        {
            var assetHelper = _serviceProvider.GetService<AssetHelper>();
            return await assetHelper.GetAllAssetNamesAsync();
        }

        public async Task<Asset> GetAssetAsync(string assetId)
        {
            var assetHelper = _serviceProvider.GetService<AssetHelper>();
            return await assetHelper.GetAssetAsync(assetId);
        }

        public async Task<JObject> GetAllLocatorsAsync()
        {
            var assetHelper = _serviceProvider.GetService<AssetHelper>();
            return await assetHelper.GetAllLocatorsAsync();
        }

        public async Task<JObject> GetAllJobsAsync()
        {
            var jobHelper = _serviceProvider.GetService<JobHelper>();
            return await jobHelper.GetAllJobsAsync();
        }

        public async Task<Job> GetJob(string jobName)
        {
            var jobHelper = _serviceProvider.GetService<JobHelper>();
            return await jobHelper.GetJob(jobName);
        }

    }
}
