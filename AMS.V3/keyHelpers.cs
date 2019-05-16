//
// Azure Media Services REST API v2 - Functions
//
// Shared Library
//

using System.Collections.Generic;
using System.Linq;

namespace AMS.V3
{
    public class KeyHelper
    {
        public static Dictionary<string, string> ReturnStorageCredentials()
        {
            MediaServicesCredentials amsCredentials = new MediaServicesCredentials();

            // Store the attached storage account to a dictionary
            Dictionary<string, string> attachedstoragecredDict = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(amsCredentials.AttachedStorageCredentials))
            {
                var tab = amsCredentials.AttachedStorageCredentials.TrimEnd(';').Split(';');
                for (int i = 0; i < tab.Count(); i += 2)
                {
                    attachedstoragecredDict.Add(tab[i], tab[i + 1]);
                }
            }
            return attachedstoragecredDict;
        }
    }
}

  