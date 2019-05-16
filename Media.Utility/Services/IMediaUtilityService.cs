using Media.Utility.Models;
using Microsoft.Azure.Management.Media.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Media.Utility.Services
{
    /// <summary>
    /// Media Utility Service will implement this interface
    /// </summary>
    public interface IMediaUtilityService
    {
        /// <summary>
        /// Encodes content via specified tranform, from MP4 file to MBR.
        /// </summary>
        /// <param name="mtc">Media Transform Context object as input/output param block</param>
        /// <returns></returns>
        Task<bool> TransformAsync(MediaTransformContext mtc);
        
        /// <summary>
        /// Publishes Assets in Media Services by creating locators, assigning DRM policy if requested
        /// </summary>
        /// <param name="mpc">Media Publish Context as input/output param block</param>
        /// <returns>Bool</returns>
        Task<bool> PublishAsync(MediaPublishContext mpc);

        /// <summary>
        /// Utility function to return all Assets stored in Azure Media Services
        /// </summary>
        /// <returns>Bool</returns>
        Task<JArray> GetAllAssetNamesAsync();

        /// <summary>
        /// Utility function to return all Locators in Azure Media Services
        /// </summary>
        /// <returns>JObject</returns>
        Task<JObject> GetAllLocatorsAsync();
        /// <summary>
        /// Utility function to list all Jobs in Azure Media Services
        /// </summary>
        /// <returns>JObject</returns>
        Task<JObject> GetAllJobsAsync();

        /// <summary>
        /// Finds the Job with the corresponding Job name in Azure Media Services
        /// </summary>
        /// <param name="jobName">Name of the Job to find</param>
        /// <returns>Job</returns>
        Task<Job> GetJob(string jobName);
    }
}
