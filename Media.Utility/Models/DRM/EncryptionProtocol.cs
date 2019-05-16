using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Media.Utility.Models.DRM
{
    public class EncryptionProtocol
    {
        public bool EncryptionEnabled { get; set; }
        public EnabledProtocols EnabledProtocols { get; set; }
        public string DefaultKeyLabel { get; set; }
        //determines whether to use multi-contentkey in both StreamingPolicy and SteramingLocator
        public bool MultiKeyEncryption { get; set; }
        public string CencH264KeyLabel { get; set; }
        public string CencH265KeyLabel { get; set; }
    }
}
