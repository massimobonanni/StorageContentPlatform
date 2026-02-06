using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ContentCreator.Utilities
{
    /// <summary>
    /// Provides utility methods for generating random metadata for documents.
    /// </summary>
    internal static class MetadataGenerator
    {
        private static readonly IEnumerable<string> DocumentTypes = new string[] { "invoice", "report","receipt" };
        private static readonly IEnumerable<string> Departments = new string[] { "finance", "logistic", "it","sale" };
        private static readonly IEnumerable<string> Sensitivities = new string[] { "public", "general", "confidential", "highly confidential" };

        private static readonly Random random = new Random(DateTime.Now.Millisecond);

        /// <summary>
        /// Generates a dictionary of metadata with random values for document type, department, and sensitivity level,
        /// along with the current date and time, and the provided content identifier.
        /// </summary>
        /// <param name="contentId">The unique identifier for the content being documented.</param>
        /// <returns>
        /// A dictionary containing the following metadata keys:
        /// <list type="bullet">
        /// <item><description>documentType - Randomly selected from available document types (invoice, report, receipt)</description></item>
        /// <item><description>department - Randomly selected from available departments (finance, logistic, it, sale)</description></item>
        /// <item><description>sensitivity - Randomly selected from available sensitivity levels (public, general, confidential, highly confidential)</description></item>
        /// <item><description>documentDate - Current date in yyyy-MM-dd format</description></item>
        /// <item><description>documentTime - Current time in HH:mm:ss format</description></item>
        /// <item><description>documentId - String representation of the provided content identifier</description></item>
        /// </list>
        /// </returns>
        public static IDictionary<string,string> GenerateMetadata (Guid contentId)
        {
            var metadata = new Dictionary<string, string>
            {
                { "documentType", DocumentTypes.ElementAt(random.Next(0, DocumentTypes.Count())) },
                { "department", Departments.ElementAt(random.Next(0, Departments.Count())) },
                { "sensitivity", Sensitivities.ElementAt(random.Next(0, Sensitivities.Count())) },
                { "documentDate", DateTime.Now.ToString("yyyy-MM-dd") },
                { "documentTime", DateTime.Now.ToString("HH:mm:ss") },
                { "documentId", contentId.ToString() }
            };
            return metadata;
        }
    }
}
