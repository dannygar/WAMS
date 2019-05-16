using System.Collections.Generic;
using Microsoft.Azure.Management.Media;

namespace AMS.V3.Models
{
    public class MediaPublishContext
    {
        public int MezzId { get; set; }
        public string containerName { get; set; }
        public string jobName { get; set; }
        public string inputAssetName { get; set; }
        public string transformedAssetName { get; set; }
        public string transformName { get; set; }
        public string jobId { get; set; }
        public Dictionary<string, string> streamingUrls { get; set; }
        public PredefinedStreamingPolicy streamingPolicy { get; set; }
        public string Errors { get; set; }
    }
}
