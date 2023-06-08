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
        private static readonly IEnumerable<string> Departments = new string[] { "finance", "logistic", "it","sale" };
        private static readonly IEnumerable<string> Sensitivities = new string[] { "public", "general", "confidential", "highly confidential" };

        private static readonly Random random = new Random(DateTime.Now.Millisecond);

        public static IDictionary<string,string> GenerateMetadata (Guid contentId)
        {
            var metadata = new Dictionary<string, string>();
            metadata.Add("documentType", DocumentTypes.ElementAt(random.Next(0, DocumentTypes.Count())));
            metadata.Add("department", Departments.ElementAt(random.Next(0, Departments.Count())));
            metadata.Add("sensitivity", Sensitivities.ElementAt(random.Next(0, Sensitivities.Count())));
            metadata.Add("documentDate", DateTime.Now.ToString("yyyy-MM-dd"));
            metadata.Add("documentTime", DateTime.Now.ToString("HH:mm:ss"));
            metadata.Add("documentId", contentId.ToString());
            return metadata;
        }
    }
}
