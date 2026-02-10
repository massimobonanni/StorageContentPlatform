using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using StorageContentPlatform.ManagementFunctions.Entities;
using StorageContentPlatform.ManagementFunctions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Services
{
    /// <summary>
    /// Analyzes Azure Storage inventory manifests to generate comprehensive statistics about blob storage usage,
    /// including access tier distribution and custom metadata aggregation.
    /// </summary>
    public class InventoryAnalyzer : IInventoryAnalyzer
    {
        /// <summary>
        /// Internal configuration class that holds the runtime configuration values for inventory analysis.
        /// </summary>
        private class Configuration
        {
            /// <summary>
            /// Gets or sets the connection string to the Azure Storage account containing inventory data.
            /// </summary>
            public string InventoryStorageConnectionString { get; set; }
            
            /// <summary>
            /// Gets or sets the collection of metadata field names to track and aggregate during analysis.
            /// </summary>
            public IEnumerable<string> MetadataFields { get; set; }
        }

        private readonly IConfiguration configuration;
        private readonly ILogger<InventoryAnalyzer> logger;
        private readonly Configuration configurationValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryAnalyzer"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration containing inventory settings.</param>
        /// <param name="logger">The logger instance for diagnostic logging.</param>
        public InventoryAnalyzer(IConfiguration configuration, ILogger<InventoryAnalyzer> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.configurationValues = new Configuration();
        }

        /// <summary>
        /// Separator characters used to parse metadata field configuration values.
        /// </summary>
        private static char[] MetadataSeparator = { '|',';',',' };

        /// <summary>
        /// Loads configuration values from the application settings into the internal configuration object.
        /// </summary>
        private void LoadConfig()
        {
            logger.LogDebug("Loading configuration values");
            this.configurationValues.InventoryStorageConnectionString = this.configuration.GetValue<string>("InventoryStorageConnectionString");
            this.configurationValues.MetadataFields = this.configuration
                .GetValue<string>("MetadataFields")
                .Split(MetadataSeparator, StringSplitOptions.RemoveEmptyEntries);
            
            logger.LogDebug("Configuration loaded. Metadata fields count: {MetadataFieldsCount}", 
                this.configurationValues.MetadataFields?.Count() ?? 0);
        }

        /// <summary>
        /// The CSV column name for blob content length in the inventory file.
        /// </summary>
        private const string ContentLengthColumn = "Content-Length";
        
        /// <summary>
        /// The CSV column name for blob access tier in the inventory file.
        /// </summary>
        private const string AccessTierColumn = "AccessTier";
        
        /// <summary>
        /// The CSV column name for blob metadata in the inventory file.
        /// </summary>
        private const string MetadataColumn = "Metadata";

        /// <summary>
        /// Analyzes an inventory manifest by processing all CSV files and aggregating statistics about blob storage usage.
        /// </summary>
        /// <param name="manifest">The inventory manifest containing references to CSV files with blob inventory data.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an <see cref="InventoryStatistics"/> 
        /// object with aggregated statistics, or null if the manifest contains no files or an error occurred.
        /// </returns>
        /// <remarks>
        /// This method downloads and parses CSV files from Azure Storage, extracting information about:
        /// - Blob counts and sizes per access tier (Hot, Cool, Cold, Archive)
        /// - Custom metadata field values and their frequencies
        /// - Overall inventory timing information
        /// </remarks>
        public async Task<InventoryStatistics> AnalyzeAsync(InventoryManifest manifest)
        {
            // Log the beginning of the analysis process with file count information
            logger.LogInformation("Starting inventory analysis for manifest with {FileCount} files", manifest.Files?.Count ?? 0);

            // Initialize result container and load configuration settings from application settings
            InventoryStatistics result = null;
            LoadConfig();

            // Process inventory files only if the manifest contains file references
            if (manifest.Files.Any())
            {
                try
                {
                    // Initialize statistics object and copy inventory timing metadata from the manifest
                    result = new InventoryStatistics();
                    result.InventoryStartTime = manifest.InventoryStartTime;
                    result.InventoryCompletionTime = manifest.InventoryCompletionTime;

                    logger.LogDebug("Processing {FileCount} inventory files", manifest.Files.Count);

                    // Algorithm: Iterate through each CSV file in the manifest
                    // Each file contains a subset of the complete inventory data
                    foreach (var file in manifest.Files)
                    {
                        logger.LogDebug("Processing inventory file: {BlobName}", file.Blob);
                        
                        // Step 1: Download the CSV file content from Azure Blob Storage
                        var blobClient = new BlobClient(this.configurationValues.InventoryStorageConnectionString,
                            manifest.DestinationContainer, file.Blob);
                        var blobContent = await blobClient.DownloadContentAsync();

                        // Step 2: Parse the CSV file using TextFieldParser for robust delimiter handling
                        using (TextFieldParser parser = new TextFieldParser(blobContent.Value.Content.ToStream()))
                        {
                            parser.TextFieldType = FieldType.Delimited;
                            parser.SetDelimiters(",");
                            var rowIndex = 0;
                            int contentLengthColumnIndex = 0, accessTierColumnIndex = 0, metadataColumnIndex = 0;
                            
                            // Step 3: Process each row in the CSV file
                            while (!parser.EndOfData)
                            {
                                // Read all fields from the current row
                                string[] fields = parser.ReadFields();

                                // Skip empty or incomplete rows to avoid processing errors
                                if (fields == null || fields.Length == 0 || 
                                    (fields.Length == 1 && string.IsNullOrWhiteSpace(fields[0])))
                                {
                                    rowIndex++;
                                    continue;
                                }

                                // Step 3a: Parse the header row (first row, index 0)
                                // Find the column indices for the fields we need to extract
                                if (rowIndex == 0)
                                {
                                    logger.LogDebug("Processing CSV header row");
                                    // Locate Content-Length column position
                                    contentLengthColumnIndex = fields.Select((elem, index) => new { elem, index })
                                            .First(p => p.elem == ContentLengthColumn)
                                            .index;
                                    // Locate AccessTier column position
                                    accessTierColumnIndex = fields.Select((elem, index) => new { elem, index })
                                            .First(p => p.elem == AccessTierColumn)
                                            .index;
                                    // Locate Metadata column position
                                    metadataColumnIndex = fields.Select((elem, index) => new { elem, index })
                                            .First(p => p.elem == MetadataColumn)
                                            .index;
                                }
                                // Step 3b: Process data rows (all rows after the header)
                                // Extract and aggregate statistics from each blob entry
                                else
                                {
                                    // Update counters for access tier distribution (Hot/Cool/Cold/Archive)
                                    ManageAccessTierCounters(result, contentLengthColumnIndex, accessTierColumnIndex, fields);
                                    // Update counters for custom metadata field values
                                    ManageMetadataCounters(result, metadataColumnIndex, fields);
                                }
                                rowIndex++;
                            }
                            
                            logger.LogDebug("Processed {RowCount} rows from file {BlobName}", rowIndex, file.Blob);
                        }
                    }
                    
                    // Log final aggregated statistics after processing all files
                    logger.LogInformation("Inventory analysis completed. Total objects: {ObjectCount}, Total size: {TotalSize} bytes", 
                        result.ObjectCount, result.TotalObjectSize);
                }
                catch (Exception ex)
                {
                    // Log any errors that occur during processing and re-throw to caller
                    logger.LogError(ex, "Error occurred during inventory analysis");
                    throw;
                    result = null;
                }
            }
            else
            {
                // Handle case where manifest contains no files to process
                logger.LogWarning("No files found in manifest for analysis");
            }
            
            // Return the aggregated statistics or null if no data was processed
            return result;
        }

        /// <summary>
        /// Processes and aggregates metadata counters from a CSV row by deserializing the metadata JSON 
        /// and tracking occurrences of configured metadata field values.
        /// </summary>
        /// <param name="result">The inventory statistics object to update with metadata counters.</param>
        /// <param name="metadataColumnIndex">The zero-based index of the metadata column in the CSV row.</param>
        /// <param name="fields">The array of field values from the current CSV row.</param>
        /// <remarks>
        /// This method deserializes JSON metadata, checks for configured metadata fields, and maintains
        /// a count of how many times each unique value appears. Empty or malformed metadata is safely ignored.
        /// </remarks>
        private void ManageMetadataCounters(InventoryStatistics result, int metadataColumnIndex, string[] fields)
        {
            var metadataField = fields[metadataColumnIndex];
            
            // Skip if metadata field is null, empty, or whitespace
            if (string.IsNullOrWhiteSpace(metadataField))
            {
                return;
            }

            try
            {
                var metadataColumn = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataField);

                // Handle case where deserialization returns null
                if (metadataColumn == null)
                {
                    return;
                }

                foreach (var item in this.configurationValues.MetadataFields)
                {
                    if (metadataColumn.ContainsKey(item))
                    {
                        var metadataValue = metadataColumn[item];

                        if (!result.MetadataList.ContainsKey(item))
                            result.MetadataList.Add(item, new Metadata() { Label = item });

                        if (!result.MetadataList[item].Counters.ContainsKey(metadataValue))
                        {
                            result.MetadataList[item].Counters.Add(metadataValue, 1);
                        }
                        else
                        {
                            result.MetadataList[item].Counters[metadataValue]++;
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Failed to deserialize metadata field: {MetadataField}", metadataField);
            }
        }

        /// <summary>
        /// Updates access tier statistics (Hot, Cool, Cold, Archive) by processing blob size and tier information from a CSV row.
        /// </summary>
        /// <param name="result">The inventory statistics object to update with access tier counters.</param>
        /// <param name="contentLengthColumnIndex">The zero-based index of the content length column in the CSV row.</param>
        /// <param name="accessTierColumnIndex">The zero-based index of the access tier column in the CSV row.</param>
        /// <param name="fields">The array of field values from the current CSV row.</param>
        /// <remarks>
        /// This method increments both the count and total size for the appropriate access tier category,
        /// as well as the overall totals across all tiers.
        /// </remarks>
        private void ManageAccessTierCounters(InventoryStatistics result, int contentLengthColumnIndex, int accessTierColumnIndex, string[] fields)
        {
            var accessTier = fields[accessTierColumnIndex].ToLower();
            var blobSize = long.Parse(fields[contentLengthColumnIndex]);
            switch (accessTier)
            {
                case "hot":
                    result.ObjectInHotCount++;
                    result.TotalObjectInHotSize += blobSize;
                    break;
                case "cool":
                    result.ObjectInCoolCount++;
                    result.TotalObjectInCoolSize += blobSize;
                    break;
                case "cold":
                    result.ObjectInColdCount++;
                    result.TotalObjectInColdSize += blobSize;
                    break;
                case "archive":
                    result.ObjectInArchiveCount++;
                    result.TotalObjectInArchiveSize += blobSize;
                    break;
            }
            result.ObjectCount++;
            result.TotalObjectSize += blobSize;
        }
    }
}
