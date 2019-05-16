using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.Management.Media.Models;

namespace AMS.V3.Models
{
    public class MediaTransformContext
    {
        public int MezzId { get; set; }
        public string ContainerName { get; set; }
        public string JobName { get; set; }
        public string InputAssetName { get; set; }
        public string TransformedAssetName { get; set; }
        public string TransformName { get; set; }
        public string OutputAssetContainer { get; set; }
        public string JobId { get; set; }
        public string Errors { get; set; }
        public IEnumerable<Preset> Presets { get; set; }
    }
}
