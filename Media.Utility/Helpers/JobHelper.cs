using FilmDove.Logger;
using Media.Utility.Data;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Media.Utility.Helpers
{
    internal class JobHelper
    {
        private MediaServices _mediaServices { get; set; }
        private AzureMediaServicesClient _azureMediaServicesClient { get; set; }

        private readonly IGraphLogger<JobHelper> _log = null;

        public JobHelper(MediaServices mediaServices, IGraphLogger<JobHelper> lLog)
        {
            _log = lLog;
            _mediaServices = mediaServices;
            var t = MediaServicesClientHelper.GetAzureMediaServicesClientAsync(_mediaServices);
            t.Wait();
            _azureMediaServicesClient = (AzureMediaServicesClient)t.Result;
        }

        public async Task<JObject> GetAllJobsAsync()
        {
            List<Job> jobList = await GetAllJobs();
            var jobs = new Dictionary<string, string>();
            foreach (var job in jobList)
            {
                jobs.Add(job.Name, job.State);
            }
            return JObject.FromObject(jobs);
        }

        public async Task<Job> GetJob(string jobName)
        {
            return await _azureMediaServicesClient.Jobs.GetAsync(_mediaServices.mediaRg, _mediaServices.mediaMdsName, MediaConstants.DefaultTransformName, jobName);
        }

        private async Task<List<Job>> GetAllJobs(int maxPages = 10)
        {
            var jobList = new List<Job>();
            string nextPageLink = string.Empty;
            Page<Job> page = null;
            do
            {
                if (string.IsNullOrEmpty(nextPageLink))
                {
                    page = (Page<Job>)await _azureMediaServicesClient.Jobs.ListAsync(_mediaServices.mediaRg, _mediaServices.mediaMdsName, MediaConstants.DefaultTransformName);
                }
                else
                {
                    page = (Page<Job>)await _azureMediaServicesClient.Jobs.ListNextAsync(nextPageLink);
                }
                if (page != null)
                {
                    jobList.AddRange(page);
                    nextPageLink = page.NextPageLink;
                }
                maxPages--;
            } while (!string.IsNullOrEmpty(nextPageLink) && maxPages > 0);
            return jobList;
        }
    }
}
