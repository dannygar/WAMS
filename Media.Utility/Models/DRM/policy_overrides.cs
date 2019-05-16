namespace Media.Utility.Models.DRM
{
    //https://docs.microsoft.com/en-us/azure/media-services/previous/media-services-widevine-license-template-overview 
    public class policy_overrides
    {
        public bool can_play { get; set; }
        public bool can_persist { get; set; }
        public bool can_renew { get; set; }
        public int rental_duration_seconds { get; set; }    //Indicates the time window while playback is permitted. A value of 0 indicates that there is no limit to the duration. Default is 0.
        public int playback_duration_seconds { get; set; }  //The viewing window of time after playback starts within the license duration. A value of 0 indicates that there is no limit to the duration. Default is 0.
        public int license_duration_seconds { get; set; }   //Indicates the time window for this specific license. A value of 0 indicates that there is no limit to the duration. Default is 0.
    }
}