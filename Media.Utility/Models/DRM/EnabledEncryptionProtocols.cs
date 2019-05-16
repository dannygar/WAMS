namespace Media.Utility.Models.DRM
{
    /// <summary>
    /// The EncryptionProtocol corresponding to CENC, CBCS and CBC
    /// </summary>
    public class EnabledEncryptionProtocols
    {
        public EncryptionProtocol CENC { get; set; }
        public EncryptionProtocol CBCS { get; set; }
        public EncryptionProtocol CBC { get; set; }
    }
}