﻿using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StorageContentPlatform.ManagementFunctions.Entities;
using StorageContentPlatform.ManagementFunctions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Services
{
    public class StorageManagementService : IPersistenceManagementService
    {
        private class Configuration
        {
            private string storageConnectionString; 
            public string StorageConnectionString { 
                get=> storageConnectionString;
                set
                {
                    storageConnectionString = value;
                    var segments= value.Split(';');
                    foreach(var segment in segments)
                    {
                        var keyValuePair = segment.Split('=',2);
                        if (keyValuePair[0].Equals("AccountName"))
                        {
                            this.AccountName = keyValuePair[1];
                        }
                        else if (keyValuePair[0].Equals("AccountKey"))
                        {
                            this.SharedKey = keyValuePair.Skip(1).Aggregate((a,b)=>a+b);
                        }
                    }   
                }
            }
            public string AccountName { get; private set; }
            public string SharedKey { get; private set; }
        }

        private readonly IConfiguration configuration;
        private readonly Configuration configurationValues;
        private readonly ILogger<StorageManagementService> logger;

        public StorageManagementService(IConfiguration configuration, ILogger<StorageManagementService> logger)
        {
            this.configuration = configuration;
            this.configurationValues = new Configuration();
            this.logger = logger;
        }

        private void LoadConfig()
        {
            this.configurationValues.StorageConnectionString = this.configuration.GetValue<string>("ContentsStorageConnectionString");
        }

        public async Task<bool> UndeleteBlobAsync(string blobUrl)
        {
            logger.LogInformation("Starting undelete operation for blob: {BlobUrl}", blobUrl);
            
            bool result = false;
            
            try
            {
                LoadConfig();
                logger.LogDebug("Configuration loaded for storage account: {AccountName}", configurationValues.AccountName);

                var credential = new StorageSharedKeyCredential(configurationValues.AccountName,
                    configurationValues.SharedKey);
                var blobClient = new BlobClient(new Uri(blobUrl), credential);
                
                logger.LogInformation("Attempting to undelete blob: {BlobUrl}", blobUrl);
                var undeleteResponse = await blobClient.UndeleteAsync();
                
                result = !undeleteResponse.IsError;
                
                if (result)
                {
                    logger.LogInformation("Successfully undeleted blob: {BlobUrl}", blobUrl);
                }
                else
                {
                    logger.LogWarning("Failed to undelete blob: {BlobUrl}. Response indicates error.", blobUrl);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occurred while undeleting blob: {BlobUrl}", blobUrl);
                result = false;
            }

            logger.LogInformation("Undelete operation completed for blob: {BlobUrl}. Result: {Result}", blobUrl, result);
            return result;
        }
    }
}
