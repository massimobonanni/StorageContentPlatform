using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ContentCreator.Utilities
{
    internal static class MetadataGenerator
    {
        private static readonly IEnumerable<string> DocumentTypes = new string[] { "invoice", "report","receipt" };
  
        public static IDictionary<string,string> GenerateMetadata (Guid contentId)
        {
            var metadata = new Dictionary<string, string>();
            metadata.Add("documentType", DocumentTypes.ElementAt(new Random().Next(0, DocumentTypes.Count())));
            metadata.Add("documentDate", DateTime.Now.ToString("yyyy-MM-dd"));
            metadata.Add("documentTime", DateTime.Now.ToString("HH:mm:ss"));
            metadata.Add("documentId", contentId.ToString());
            return metadata;
        }
    }
}
