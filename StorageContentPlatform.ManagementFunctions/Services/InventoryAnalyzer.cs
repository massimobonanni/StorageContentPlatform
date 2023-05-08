using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.FileIO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
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
        private readonly Configuration configurationValues;

        public InventoryAnalyzer(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.configurationValues = new Configuration();
        }

        private void LoadConfig()
        {
            this.configurationValues.InventoryStorageConnectionString = this.configuration.GetValue<string>("InventoryStorageConnectionString");
            this.configurationValues.MetadataFields = this.configuration.GetValue<string>("MetadataFields").Split("|",StringSplitOptions.RemoveEmptyEntries);
        }

        private const string ContentLengthColumn = "Content-Length";
        private const string AccessTierColumn = "AccessTier";
        private const string MetadataColumn = "Metadata";

        public async Task<InventoryStatistics> AnalyzeAsync(InventoryManifest manifest)
        {
            InventoryStatistics result = null;
            LoadConfig();

            if (manifest.Files.Any())
            {
                try
                {
                    result = new InventoryStatistics();
                    result.InventoryStartTime = manifest.InventoryStartTime;
                    result.InventoryCompletionTime = manifest.InventoryCompletionTime;

                    foreach (var file in manifest.Files)
                    {
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
                                if (rowIndex == 0)
                                {
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
                        }
                    }
                }
                catch
                {
                    result = null;
                }
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
