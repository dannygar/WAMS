using Newtonsoft.Json;

namespace Data.Entities
{
    public class Ocr
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("outputStorage")]
        public string OutputStorage { get; set; }
    }
}
