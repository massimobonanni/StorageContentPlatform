using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StorageContentPlatform.ContentCreator.Interfaces
{
    /// <summary>
    /// Defines a contract for persistence providers that handle content storage operations.
    /// </summary>
    public interface IPersistanceProvider
    {
        /// <summary>
        /// Asynchronously saves content with the specified name and optional metadata.
        /// </summary>
        /// <param name="contentName">The name or identifier for the content to be saved.</param>
        /// <param name="content">The content data to be persisted.</param>
        /// <param name="metadata">Optional metadata key-value pairs associated with the content. Default is null.</param>
        /// <param name="token">A cancellation token to observe while waiting for the task to complete. Default is default.</param>
        /// <returns>
        /// A task that represents the asynchronous save operation. 
        /// The task result contains <c>true</c> if the content was saved successfully; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> SaveContentAsync(string contentName, string content, IDictionary<string, string> metadata = null, CancellationToken token = default);
    }
}
