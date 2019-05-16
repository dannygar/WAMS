using Newtonsoft.Json;

namespace Data.Entities
{
    public class VideoAnnotation
    {
        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("outputStorage")]
        public string OutputStorage { get; set; }

    }
}
