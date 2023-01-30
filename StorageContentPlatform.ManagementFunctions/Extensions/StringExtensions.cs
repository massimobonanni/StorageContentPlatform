using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class StringExtensions
    {
        public static void ExtractContainerAndBlobName(this string manifestBlobUrl, 
            out string containerName, out string blobName)
        {
            containerName = string.Empty;
            blobName = string.Empty;
            var urlParts = manifestBlobUrl.Split('/');
            if (urlParts.Length > 3)
            {
                containerName = urlParts[3];
                blobName = string.Join('/', urlParts.Skip(4));
            }
        }
    }
}
