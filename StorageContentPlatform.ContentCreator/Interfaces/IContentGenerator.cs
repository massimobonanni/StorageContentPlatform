using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ContentCreator.Interfaces
{
    public interface IContentGenerator
    {
        Task<bool> GenerateContentsAsync();
    }
}
