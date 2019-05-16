using Media.Utility.Data;
using Media.Utility.Helpers;
using Media.Utility.Models;
using Media.Utility.Operations;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Media.Utility.Services
{
    public class MediaUtilityService : IMediaUtilityService
    {
        private readonly IServiceProvider _serviceProvider;

        public MediaUtilityService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Registers the classes needed by the libary within the DependencyInjection collection of the calling app.
        /// </summary>
        /// <param name="serviceCollection"></param>
        public static void UseMediaUtilityService(IServiceCollection serviceCollection, MediaServices mediaServices)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.AddSingleton<MediaServices>(mediaServices);
            serviceCollection.AddTransient<ContentKeyPolicyHelper>();
            serviceCollection.AddTransient<AssetHelper>();
            serviceCollection.AddTransient<JobHelper>();
            serviceCollection.AddTransient<MediaTransform>();
            serviceCollection.AddTransient<MediaPublish>();
            serviceCollection.AddTransient<IMediaUtilityService, Media.Utility.Services.MediaUtilityService>();

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

        public async Task<JObject> GetAllLocatorsAsync()
        {
            var assetHelper = _serviceProvider.GetService<AssetHelper>();
            return await assetHelper.GetAllLoctorsAsync();
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
