using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StorageContentPlatform.ContentCreator.Interfaces
{
    /// <summary>
    /// Defines the contract for content generation operations.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface are responsible for generating and managing content 
    /// within the Storage Content Platform. The interface provides asynchronous operations 
    /// to support non-blocking content generation workflows.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class MyContentGenerator : IContentGenerator
    /// {
    ///     public async Task&lt;bool&gt; GenerateContentsAsync(CancellationToken token = default)
    ///     {
    ///         // Implementation logic here
    ///         return true;
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IContentGenerator
    {
        /// <summary>
        /// Asynchronously generates contents.
        /// </summary>
        /// <param name="token">
        /// A cancellation token that can be used to cancel the asynchronous operation. 
        /// Default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains <c>true</c> if the content generation was successful; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled via the <paramref name="token"/>.
        /// </exception>
        Task<bool> GenerateContentsAsync(CancellationToken token=default);
    }
}
