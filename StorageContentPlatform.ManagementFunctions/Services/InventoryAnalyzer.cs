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
    public class InventoryAnalyzer : IInventoryAnalyzer
    {
        private class Configuration
        {
            public string InventoryStorageConnectionString { get; set; }
            public IEnumerable<string> MetadataFields { get; set; }
        }

        private readonly IConfiguration configuration;
        private readonly ILogger<InventoryAnalyzer> logger;
        private readonly Configuration configurationValues;

        public InventoryAnalyzer(IConfiguration configuration, ILogger<InventoryAnalyzer> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.configurationValues = new Configuration();
        }

        private static char[] MetadataSeparator = { '|',';',',' };

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

        private const string ContentLengthColumn = "Content-Length";
        private const string AccessTierColumn = "AccessTier";
        private const string MetadataColumn = "Metadata";

        public async Task<InventoryStatistics> AnalyzeAsync(InventoryManifest manifest)
        {
            logger.LogInformation("Starting inventory analysis for manifest with {FileCount} files", manifest.Files?.Count ?? 0);
            
            InventoryStatistics result = null;
            LoadConfig();

            if (manifest.Files.Any())
            {
                try
                {
                    result = new InventoryStatistics();
                    result.InventoryStartTime = manifest.InventoryStartTime;
                    result.InventoryCompletionTime = manifest.InventoryCompletionTime;

                    logger.LogDebug("Processing {FileCount} inventory files", manifest.Files.Count);

                    foreach (var file in manifest.Files)
                    {
                        logger.LogDebug("Processing inventory file: {BlobName}", file.Blob);
                        
                        var blobClient = new BlobClient(this.configurationValues.InventoryStorageConnectionString,
                            manifest.DestinationContainer, file.Blob);
                        var blobContent = await blobClient.DownloadContentAsync();

                        using (TextFieldParser parser = new TextFieldParser(blobContent.Value.Content.ToStream()))
                        {
                            parser.TextFieldType = FieldType.Delimited;
                            parser.SetDelimiters(",");
                            var rowIndex = 0;
                            int contentLengthColumnIndex = 0, accessTierColumnIndex = 0, metadataColumnIndex = 0;
                            
                            while (!parser.EndOfData)
                            {
                                //Processing row
                                string[] fields = parser.ReadFields();

                                // Skip empty or incomplete rows
                                if (fields == null || fields.Length == 0 || 
                                    (fields.Length == 1 && string.IsNullOrWhiteSpace(fields[0])))
                                {
                                    rowIndex++;
                                    continue;
                                }

                                if (rowIndex == 0)
                                {
                                    logger.LogDebug("Processing CSV header row");
                                    contentLengthColumnIndex = fields.Select((elem, index) => new { elem, index })
                                            .First(p => p.elem == ContentLengthColumn)
                                            .index;
                                    accessTierColumnIndex = fields.Select((elem, index) => new { elem, index })
                                            .First(p => p.elem == AccessTierColumn)
                                            .index;
                                    metadataColumnIndex = fields.Select((elem, index) => new { elem, index })
                                            .First(p => p.elem == MetadataColumn)
                                            .index;
                                }
                                else
                                {
                                    ManageAccessTierCounters(result, contentLengthColumnIndex, accessTierColumnIndex, fields);
                                    ManageMetadataCounters(result, metadataColumnIndex, fields);
                                }
                                rowIndex++;
                            }
                            
                            logger.LogDebug("Processed {RowCount} rows from file {BlobName}", rowIndex, file.Blob);
                        }
                    }
                    
                    logger.LogInformation("Inventory analysis completed. Total objects: {ObjectCount}, Total size: {TotalSize} bytes", 
                        result.ObjectCount, result.TotalObjectSize);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred during inventory analysis");
                    result = null;
                }
            }
            else
            {
                logger.LogWarning("No files found in manifest for analysis");
            }
            
            return result;
        }

        private void ManageMetadataCounters(InventoryStatistics result, int metadataColumnIndex, string[] fields)
        {
            var metadataField = fields[metadataColumnIndex];
            var metadataColumn = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataField);

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
