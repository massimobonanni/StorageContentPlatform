using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Entities
{
    public class InventoryStatistics
    {
        public DateTimeOffset InventoryCompletionTime { get; set; }
        public DateTimeOffset InventoryStartTime { get; set; }
        public long ObjectCount { get; set; }
        public long TotalObjectSize { get; set; }
        public long ObjectInHotCount { get; set; }
        public long TotalObjectInHotSize { get; set; }
        public long ObjectInCoolCount { get; set; }
        public long TotalObjectInCoolSize { get; set; }
        public long ObjectInArchiveCount { get; set; }
        public long TotalObjectInArchiveSize { get; set; }
    }
}
