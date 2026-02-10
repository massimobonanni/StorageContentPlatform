using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// Provides extension methods for <see cref="string"/> operations.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Extracts the container name and blob name from an Azure Storage blob URL.
        /// </summary>
        /// <param name="manifestBlobUrl">The blob URL to parse. Expected format: scheme://host/account/container/blob/path</param>
        /// <param name="containerName">When this method returns, contains the container name extracted from the URL, or an empty string if extraction fails.</param>
        /// <param name="blobName">When this method returns, contains the blob name (including path) extracted from the URL, or an empty string if extraction fails.</param>
        /// <remarks>
        /// This method splits the URL by '/' and expects the container name at index 3 and the blob name starting at index 4.
        /// If the URL has fewer than 4 parts, both output parameters will be empty strings.
        /// </remarks>
        /// <example>
        /// <code>
        /// string url = "https://mystorageaccount.blob.core.windows.net/mycontainer/path/to/blob.json";
        /// url.ExtractContainerAndBlobName(out string container, out string blob);
        /// // container = "mycontainer"
        /// // blob = "path/to/blob.json"
        /// </code>
        /// </example>
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
