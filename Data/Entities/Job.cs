using Newtonsoft.Json;

namespace Data.Entities
{
    public class Job
    {
        [JsonProperty("assetId")]
        public string AssetId { get; set; }

        [JsonProperty("indexV2")]
        public IndexV2 IndexV2 { get; set; }

        [JsonProperty("mes")]
        public Mes Mes { get; set; }

        [JsonProperty("ocr")]
        public Ocr Ocr { get; set; }

        [JsonProperty("summarization")]
        public Summarization Summarization { get; set; }

        [JsonProperty("useEncoderOutputForAnalytics")]
        public bool UseEncoderOutputForAnalytics { get; set; }

        [JsonProperty("videoAnnotation")]
        public VideoAnnotation VideoAnnotation { get; set; }
    }
}
