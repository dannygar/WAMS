namespace Media.Utility.Models.DRM
{
    public class content_key_spec
    {
        public string track_type { get; set; }
        public int security_level { get; set; }
        public output_protection required_output_protection { get; set; }
    }
}