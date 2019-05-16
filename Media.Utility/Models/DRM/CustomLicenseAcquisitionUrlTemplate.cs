namespace Media.Utility.Models.DRM
{

    public class CustomLicenseAcquisitionUrlTemplate
    {
        public string PlayReady { get; set; }
        public string Widevine { get; set; }
        public string FairPlay { get; set; }  //use skd:// instead of https:// This will appear in HLS playlist(s). In addition, diagnoverly.cs relies on skd: to parse FPS LA_URL.
        public string AES128 { get; set; }
    }
}