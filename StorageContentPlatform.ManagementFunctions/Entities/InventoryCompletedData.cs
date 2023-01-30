using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Entities
{
    public class InventoryCompletedData
    {
        public DateTime ScheduleDateTime { get; set; }
        public string AccountName { get; set; }
        public string RuleName { get; set; }
        public string PolicyRunStatus { get; set; }
        public string PolicyRunStatusMessage { get; set; }
        public string PolicyRunId { get; set; }
        public string ManifestBlobUrl { get; set; }
    }
}
