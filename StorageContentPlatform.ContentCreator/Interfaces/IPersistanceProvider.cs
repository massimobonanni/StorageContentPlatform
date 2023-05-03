using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ContentCreator.Interfaces
{
    public interface IPersistanceProvider
    {
        Task<bool> SaveContentAsync(string contentName, string content, IDictionary<string, string> metadata = null);
    }
}
