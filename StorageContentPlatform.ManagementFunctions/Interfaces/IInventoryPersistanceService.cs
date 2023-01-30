using StorageContentPlatform.ManagementFunctions.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Interfaces
{
    public interface IInventoryPersistanceService
    {
        Task<InventoryManifest> ReadInventoryManifestFile(string manifestBlobUrl);
        Task<bool> SaveAsync(InventoryStatistics statistics);
    }
}
