using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
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

        public StorageManagementService(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.configurationValues = new Configuration();
        }

        private void LoadConfig()
        {
            this.configurationValues.StorageConnectionString = this.configuration.GetValue<string>("ContentsStorageConnectionString");
        }

        public async Task<bool> UndeleteBlobAsync(string blobUrl)
        {
            bool result = false;
            LoadConfig();

            var credential= new StorageSharedKeyCredential(this.configurationValues.AccountName,
                this.configurationValues.SharedKey);
            var blobClient = new BlobClient(new Uri(blobUrl), credential);
            var undeleteResponse= await blobClient.UndeleteAsync();
            result = !undeleteResponse.IsError;

            return result;

        }
    }
}
