using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Entities
{
    public class InventoryManifest
    {
        public string DestinationContainer { get; set; }
        public string Endpoint { get; set; }
        public List<File> Files { get; set; }
        public DateTimeOffset InventoryCompletionTime { get; set; }
        public DateTimeOffset InventoryStartTime { get; set; }
        public RuleDefinition RuleDefinition { get; set; }
        public string RuleName { get; set; }
        public string Status { get; set; }
        public Summary Summary { get; set; }
        public string Version { get; set; }
    }

    public class File
    {
        public string Blob { get; set; }
        public long Size { get; set; }
    }

    public class Filters
    {
        public List<string> BlobTypes { get; set; }
        public bool IncludeBlobVersions { get; set; }
        public bool IncludeSnapshots { get; set; }
        public List<string> PrefixMatch { get; set; }
    }

    public class RuleDefinition
    {
        public Filters Filters { get; set; }
        public string Format { get; set; }
        public string ObjectType { get; set; }
        public string Schedule { get; set; }
        public List<string> SchemaFields { get; set; }
    }

    public class Summary
    {
        public long ObjectCount { get; set; }
        public long TotalObjectSize { get; set; }
    }


}
