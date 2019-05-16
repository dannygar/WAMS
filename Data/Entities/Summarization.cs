using Newtonsoft.Json;

namespace Data.Entities
{
    public class Summarization
    {
        [JsonProperty("duration")]
        public double Duration { get; set; }

        [JsonProperty("outputStorage")]
        public string OutputStorage { get; set; }

    }
}
