using System;
using System.Collections.Generic;
using System.Text;

namespace Media.Utility.Models
{
    public class MediaTransformContext
    {
        public int MezzId { get; set; }
        public string containerName { get; set; }
        public string jobName { get; set; }
        public string inputAssetName { get; set; }
        public string transformedAssetName { get; set; }
        public string transformName { get; set; }
        public string outputAssetContainer { get; set; }
        public string jobId { get; set; }
        public string Errors { get; set; }
    }
}
