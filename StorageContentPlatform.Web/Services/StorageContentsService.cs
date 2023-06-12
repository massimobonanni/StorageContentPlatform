using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using StorageContentPlatform.Web.Entities;
using StorageContentPlatform.Web.Interfaces;
using System.Net;

namespace StorageContentPlatform.Web.Services
{
    public class StorageContentsService : IContentsService
    {
        private class Configuration
        {
            public string StorageConnectionString { get; set; }

            public IEnumerable<string> ContainerTypes { get; set; }
        }

        private readonly IConfiguration configuration;
        private readonly Configuration configurationValues;

        public StorageContentsService(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.configurationValues = new Configuration();
        }

        private void LoadConfig()
        {
            this.configurationValues.StorageConnectionString = this.configuration.GetValue<string>("StorageConnectionString");
            this.configurationValues.ContainerTypes = this.configuration.GetValue<string>("ContainerTypes")
                    .ToLower()
                    .Split("|", StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.TrimEntries);
        }

        public async Task<IEnumerable<ContainerInfo>> GetContainersAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<ContainerInfo>();
            LoadConfig();

            var blobServiceClient = CreateBlobServiceClient();

            var resultSegment = blobServiceClient
                    .GetBlobContainersAsync(BlobContainerTraits.Metadata, null,cancellationToken)
                    .AsPages(default, 100);

            await foreach (Azure.Page<BlobContainerItem> containerPage in resultSegment)
            {
                foreach (BlobContainerItem containerItem in containerPage.Values)
                {
                    if (containerItem.HasMetadataValues("containerType",this.configurationValues.ContainerTypes))
                    {
                        var containerInfo = new ContainerInfo();
                        containerInfo.Name = containerItem.Name;
                        containerInfo.LastModified = containerItem.Properties.LastModified;
                        if (containerItem.Properties.Metadata != null && containerItem.Properties.Metadata.Any())
                            containerInfo.Metadata = containerItem.Properties.Metadata
                                     .Select((k, v) => $"{k}={v}").Aggregate((a, b) => $"{a};{b}");

                        result.Add(containerInfo);
                    }
                }
            }
            return result;
        }

        public async Task<IEnumerable<Entities.BlobInfo>> GetBlobsAsync(string containerName, DateTime date, CancellationToken cancellationToken = default)
        {
            var result = new List<Entities.BlobInfo>();
            LoadConfig();

            var blobServiceClient = CreateBlobServiceClient();

            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var blobPrefix = date.ToString("yyyyMMdd");

            var resultSegment = containerClient
                    .GetBlobsAsync(BlobTraits.All, BlobStates.None, blobPrefix, cancellationToken)
                    .AsPages(default, 100);

            await foreach (var blobPage in resultSegment)
            {
                foreach (var blob in blobPage.Values)
                {
                    var blobInfo = new Entities.BlobInfo();
                    blobInfo.Name = blob.Name;
                    blobInfo.LastModified = blob.Properties.LastModified;
                    blobInfo.ReplicationPolicyId = blob.ObjectReplicationSourceProperties?[0].PolicyId;
                    blobInfo.ReplicationRuleId = blob.ObjectReplicationSourceProperties?[0].Rules?[0].RuleId;
                    blobInfo.ReplicationStatus = blob.ObjectReplicationSourceProperties?[0].Rules?[0].ReplicationStatus.ToString();
                    blobInfo.Size = blob.Properties.ContentLength;
                    blobInfo.Tier = blob.Properties.AccessTier.HasValue ?
                        blob.Properties.AccessTier.Value.ToString() : null;

                    result.Add(blobInfo);
                }
            }
            return result.OrderByDescending(b => b.LastModified);
        }

        public async Task<BlobContent> GetBlobAsync(string containerName, string blobName,CancellationToken cancellationToken)
        {
            var result = new BlobContent();
            result.Name = blobName;
            LoadConfig();
            
            var blobServiceClient = CreateBlobServiceClient();

            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = containerClient.GetBlobClient(blobName);

            var blobContent = await blobClient.DownloadContentAsync(cancellationToken);

            if (blobContent.HasValue)
                result.Content = blobContent.Value.Content.ToString();

            var properties = await blobClient.GetPropertiesAsync(null, cancellationToken) ;

            if (properties.HasValue)
                result.Metadata = properties.Value.Metadata;

            return result;
        }

        private BlobServiceClient CreateBlobServiceClient()
        {
            // For more info about BlobClientOptions look at https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobclientoptions?view=azure-dotnet
            BlobClientOptions options = new()
            {
                Retry =
                {
                        Delay = TimeSpan.FromSeconds(2),
                        MaxRetries = 5,
                        Mode = RetryMode.Exponential,
                        MaxDelay = TimeSpan.FromSeconds(10)
                },
                GeoRedundantSecondaryUri = new Uri(GetSecondaryUrl())
            };

            var blobServiceClient = new BlobServiceClient(this.configurationValues.StorageConnectionString, options);
            return blobServiceClient;
        }

        private string GetSecondaryUrl()
        {
            string secondaryUrl = null;
            var segments = this.configurationValues.StorageConnectionString.Split(";");
            var accountNameSegment = segments.Where(s=>s.ToLower().StartsWith("accountname")).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(accountNameSegment))
            {
                var accountName = accountNameSegment.Split("=")[1];
                secondaryUrl = $"https://{accountName}-secondary.blob.core.windows.net";
            }
            return secondaryUrl;
        }
    }
}
