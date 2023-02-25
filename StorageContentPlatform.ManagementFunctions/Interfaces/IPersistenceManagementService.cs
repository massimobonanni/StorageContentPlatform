﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Interfaces
{
    public interface IPersistenceManagementService
    {
        Task<bool> UndeleteBlobAsync(string blobUrl);
    }
}
