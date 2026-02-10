using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using StorageContentPlatform.Web.Entities;
using StorageContentPlatform.Web.Interfaces;

namespace StorageContentPlatform.Web.Services
{
    /// <summary>
    /// Service implementation for managing Azure Blob Storage contents.
    /// Provides methods to retrieve containers, blobs, and blob content with support for geo-redundant storage and retry policies.
    /// </summary>
    public class StorageContentsService : IContentsService
    {
        /// <summary>
        /// Configuration settings for Azure Blob Storage connection and container filtering.
        /// </summary>
        private class Configuration
        {
            /// <summary>
            /// Gets or sets the Azure Storage connection string.
            /// </summary>
            public string? StorageConnectionString { get; set; }
            
            /// <summary>
            /// Gets or sets the Azure Storage account name.
            /// </summary>
            public string? StorageAccountName { get; set; }
            
            /// <summary>
            /// Gets or sets the collection of container types to filter.
            /// </summary>
            public IEnumerable<string>? ContainerTypes { get; set; }
        }

        private readonly IConfiguration configuration;
        private readonly Configuration configurationValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageContentsService"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration instance.</param>
        public StorageContentsService(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.configurationValues = new Configuration();
        }


        #region [ Public Methods - IContentsService implementation ]
        
        /// <summary>
        /// Retrieves all blob containers from Azure Storage that match the configured container types.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A collection of <see cref="ContainerInfo"/> objects representing the filtered containers.</returns>
        public async Task<IEnumerable<ContainerInfo>> GetContainersAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<ContainerInfo>();
            LoadConfig();
            BlobServiceClient blobServiceClient = CreateBlobServiceClient();

            var resultSegment = blobServiceClient
                    .GetBlobContainersAsync(BlobContainerTraits.Metadata, null, cancellationToken)
                    .AsPages(default, 100);

            await foreach (Azure.Page<BlobContainerItem> containerPage in resultSegment)
            {
                foreach (BlobContainerItem containerItem in containerPage.Values)
                {
                    if (containerItem.HasMetadataValues("containerType", this.configurationValues.ContainerTypes))
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

        /// <summary>
        /// Retrieves all blobs from a specified container that match the provided date prefix.
        /// </summary>
        /// <param name="containerName">The name of the container to search.</param>
        /// <param name="date">The date used to filter blobs by prefix (formatted as yyyyMMdd).</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A collection of <see cref="BlobInfo"/> objects ordered by last modified date in descending order.</returns>
        public async Task<IEnumerable<Entities.BlobInfo>> GetBlobsAsync(string containerName, DateTime date, CancellationToken cancellationToken = default)
        {
            var result = new List<Entities.BlobInfo>();
            LoadConfig();

            BlobContainerClient containerClient = CreateBlobContainerClient(containerName);

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

        /// <summary>
        /// Downloads a specific blob from a container including its content and metadata.
        /// </summary>
        /// <param name="containerName">The name of the container containing the blob.</param>
        /// <param name="blobName">The name of the blob to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="BlobContent"/> object containing the blob's content and metadata.</returns>
        public async Task<BlobContent> GetBlobAsync(string containerName, string blobName, CancellationToken cancellationToken)
        {
            var result = new BlobContent();
            result.Name = blobName;
            LoadConfig();

            BlobClient blobClient = CreateBlobClient(containerName, blobName);

            var blobContent = await blobClient.DownloadContentAsync(cancellationToken);

            if (blobContent.HasValue)
                result.Content = blobContent.Value.Content.ToString();

            var properties = await blobClient.GetPropertiesAsync(null, cancellationToken);

            if (properties.HasValue)
                result.Metadata = properties.Value.Metadata;

            return result;
        }

        #endregion [ Public Methods - IContentsService implementation ]

        #region [ Private Methods ]
        
        /// <summary>
        /// Loads configuration values from the application configuration including storage connection string, account name, and container types.
        /// </summary>
        private void LoadConfig()
        {
            this.configurationValues.StorageConnectionString = this.configuration.GetValue<string>("StorageConnectionString");
            this.configurationValues.StorageAccountName = this.configuration.GetValue<string>("StorageAccountName");
            this.configurationValues.ContainerTypes = this.configuration.GetValue<string>("ContainerTypes")?
                    .ToLower()
                    .Split("|", StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.TrimEntries);
        }

        /// <summary>
        /// Creates a <see cref="BlobServiceClient"/> instance using either a connection string or DefaultAzureCredential.
        /// </summary>
        /// <returns>A configured <see cref="BlobServiceClient"/> instance.</returns>
        private BlobServiceClient CreateBlobServiceClient()
        {
            BlobServiceClient? blobServiceClient = null;

            if (!string.IsNullOrWhiteSpace(this.configurationValues.StorageConnectionString))
            {
                blobServiceClient = new BlobServiceClient(
                    this.configurationValues.StorageConnectionString,
                    GetClientOptions());
            }
            else
            {
                blobServiceClient = new BlobServiceClient(
                    new Uri($"https://{this.configurationValues.StorageAccountName}.blob.core.windows.net"),
                    new DefaultAzureCredential(),
                    GetClientOptions());
            }

            return blobServiceClient;
        }

        /// <summary>
        /// Creates a <see cref="BlobContainerClient"/> instance for a specific container using either a connection string or DefaultAzureCredential.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>A configured <see cref="BlobContainerClient"/> instance.</returns>
        private BlobContainerClient CreateBlobContainerClient(string containerName)
        {
            BlobContainerClient? blobContainerClient = null;
            if (!string.IsNullOrWhiteSpace(this.configurationValues.StorageConnectionString))
            {
                blobContainerClient = new BlobContainerClient(
                    this.configurationValues.StorageConnectionString,
                    containerName,
                    GetClientOptions());
            }
            else
            {
                blobContainerClient = new BlobContainerClient(
                    new Uri($"https://{this.configurationValues.StorageAccountName}.blob.core.windows.net/{containerName}"),
                    new DefaultAzureCredential(),
                    GetClientOptions());
            }
            return blobContainerClient;
        }

        /// <summary>
        /// Creates a <see cref="BlobClient"/> instance for a specific blob using either a connection string or DefaultAzureCredential.
        /// </summary>
        /// <param name="containerName">The name of the container containing the blob.</param>
        /// <param name="blobName">The name of the blob.</param>
        /// <returns>A configured <see cref="BlobClient"/> instance.</returns>
        private BlobClient CreateBlobClient(string containerName, string blobName)
        {
            BlobClient? blobClient = null;

            if (!string.IsNullOrWhiteSpace(this.configurationValues.StorageConnectionString))
            {
                blobClient = new BlobClient(
                    this.configurationValues.StorageConnectionString,
                    containerName,
                    blobName,
                    GetClientOptions());
            }
            else
            {
                blobClient = new BlobClient(
                    new Uri($"https://{this.configurationValues.StorageAccountName}.blob.core.windows.net/{containerName}/{blobName}"),
                    new DefaultAzureCredential(),
                    GetClientOptions());
            }

            return blobClient;
        }

        /// <summary>
        /// Creates and configures <see cref="BlobClientOptions"/> with retry policies and geo-redundant secondary URI support.
        /// </summary>
        /// <returns>A configured <see cref="BlobClientOptions"/> instance with exponential retry policy.</returns>
        private BlobClientOptions GetClientOptions()
        {
            var options = new BlobClientOptions();
            options.Retry.Delay = TimeSpan.FromMilliseconds(250);
            options.Retry.MaxRetries = 5;
            options.Retry.Mode = RetryMode.Exponential;
            options.Retry.MaxDelay = TimeSpan.FromSeconds(5);
            var secondaryUrl = GetSecondaryUrl();
            if (!string.IsNullOrWhiteSpace(secondaryUrl))
            {
              options.GeoRedundantSecondaryUri = new Uri(secondaryUrl);  
            }
            return options;
        }

        /// <summary>
        /// Constructs the secondary URL for geo-redundant storage access.
        /// </summary>
        /// <returns>The secondary storage account URL, or null if it cannot be determined.</returns>
        private string? GetSecondaryUrl()
        {
            string? secondaryUrl = null;
            if (!string.IsNullOrWhiteSpace(configurationValues.StorageConnectionString))
            {
                var segments = configurationValues.StorageConnectionString.Split(";");
                var accountNameSegment = segments.Where(s => s.ToLower().StartsWith("accountname")).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(accountNameSegment))
                {
                    var accountName = accountNameSegment.Split("=")[1];
                    secondaryUrl = $"https://{accountName}-secondary.blob.core.windows.net";
                }
            }
            else
            {
                secondaryUrl = $"https://{configurationValues.StorageAccountName}-secondary.blob.core.windows.net";
            }
            return secondaryUrl;
        }

        #endregion [ Private Methods ]
    }
}
