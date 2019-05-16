namespace Media.Utility.Models.DRM
{
    public enum STSType
    {
        AAD,                         //use AAD as STS, with key rollover
        Symmetric,                   //use custom STS with static symmetric key
        Asymmetric,                  //use custom STS with fixed x.509 cert
        AsymmetricWithKeyDiscovery,  //use custom STS with JWKS discovery instead of fixed x.509 cert, supporting key rollover
        None                         //open restriction
    }
}