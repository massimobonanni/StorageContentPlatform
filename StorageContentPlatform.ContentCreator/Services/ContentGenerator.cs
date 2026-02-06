using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StorageContentPlatform.ContentCreator.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StorageContentPlatform.ContentCreator.Services
{
    /// <summary>
    /// Generates random content blobs and persists them using the configured persistence provider.
    /// </summary>
    public class ContentGenerator : IContentGenerator
    {
        /// <summary>
        /// Configuration settings for content generation.
        /// </summary>
        private class Configuration
        {
            /// <summary>
            /// Gets or sets the minimum size in kilobytes for generated blobs.
            /// </summary>
            public int BlobMinimumSizeInKb { get; set; }

            /// <summary>
            /// Gets or sets the maximum size in kilobytes for generated blobs.
            /// </summary>
            public int BlobMaximumSizeInKb { get; set; }

            /// <summary>
            /// Gets or sets the total cumulative size in kilobytes to generate before stopping.
            /// </summary>
            public int ContentsSizeForGenerationInKb { get; set; }
        }

        private readonly IPersistanceProvider persistanceProvider;
        private readonly IConfiguration configuration;
        private readonly Configuration configurationValues;
        private readonly ILogger<ContentGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentGenerator"/> class.
        /// </summary>
        /// <param name="persistanceProvider">The persistence provider used to save generated content.</param>
        /// <param name="configuration">The application configuration containing generation settings.</param>
        /// <param name="loggerFactory">The factory used to create logger instances.</param>
        public ContentGenerator(IPersistanceProvider persistanceProvider,
            IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this.persistanceProvider = persistanceProvider;
            this.configuration = configuration;
            configurationValues = new Configuration();
            this.logger = loggerFactory.CreateLogger<ContentGenerator>();
        }

        /// <summary>
        /// Generates random content blobs until the cumulative size reaches the configured threshold.
        /// Each content blob is assigned a unique identifier, timestamped filename, and metadata.
        /// </summary>
        /// <param name="token">Optional cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation, containing a boolean indicating success.</returns>
        public async Task<bool> GenerateContentsAsync(CancellationToken token=default)
        {
            var result = true;
            LoadConfig();

            var cumulativeSize = 0.0;
            while (cumulativeSize < this.configurationValues.ContentsSizeForGenerationInKb)
            {
                var contentId = Guid.NewGuid();
                var contentName = @$"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{contentId}.txt";
                var content = Utilities.ContentGenerator.GenerateRandomContent(
                    this.configurationValues.BlobMinimumSizeInKb, 
                    this.configurationValues.BlobMaximumSizeInKb,
                    out var size);
                var metadata = Utilities.MetadataGenerator.GenerateMetadata(contentId);

                this.logger.LogInformation("Saving content {ContentName} with size {Size} Kb", contentName, size);
                await persistanceProvider.SaveContentAsync(contentName, content, metadata);
                this.logger.LogInformation("Saved content {ContentName} with size {Size} Kb", contentName, size);

                cumulativeSize += size;
            }

            return result;
        }

        /// <summary>
        /// Loads configuration values from the application configuration into the internal configuration object.
        /// </summary>
        private void LoadConfig()
        {
            this.logger.LogInformation("Loading configuration");
            this.configurationValues.BlobMinimumSizeInKb = this.configuration.GetValue<int>("BlobMinimumSizeInKb");
            this.configurationValues.BlobMaximumSizeInKb = this.configuration.GetValue<int>("BlobMaximumSizeInKb");
            this.configurationValues.ContentsSizeForGenerationInKb = this.configuration.GetValue<int>("ContentsSizeForGenerationInKb");
        }
    }
}
