using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Entities
{
    public class Metadata
    {
        public string Label { get; set; }

        public IDictionary<string, long> Counters { get; set; } = new Dictionary<string, long>()
    }
}
