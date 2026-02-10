using StorageContentPlatform.ManagementFunctions.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Interfaces
{
    /// <summary>
    /// Defines a contract for analyzing Azure Storage inventory manifests to generate comprehensive statistics.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface process Azure Storage inventory manifest data to produce
    /// detailed analytics including object counts, size distributions across access tiers (Hot, Cool, Cold, Archive),
    /// and custom metadata aggregation.
    /// </remarks>
    public interface IInventoryAnalyzer
    {
        /// <summary>
        /// Analyzes an Azure Storage inventory manifest asynchronously to generate comprehensive statistics.
        /// </summary>
        /// <param name="manifest">The inventory manifest containing blob storage data to analyze, including
        /// inventory files, completion times, and summary information.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an 
        /// <see cref="InventoryStatistics"/> object with detailed metrics including:
        /// <list type="bullet">
        /// <item><description>Total object count and size</description></item>
        /// <item><description>Access tier distribution (Hot, Cool, Cold, Archive)</description></item>
        /// <item><description>Aggregated custom metadata fields</description></item>
        /// </list>
        /// </returns>
        Task<InventoryStatistics> AnalyzeAsync(InventoryManifest manifest);
    }
}
