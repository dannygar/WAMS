using Newtonsoft.Json;

namespace Data.Entities
{
    public class Mes
    {
        [JsonProperty("preset")]
        public string Preset { get; set; }
    }
}
