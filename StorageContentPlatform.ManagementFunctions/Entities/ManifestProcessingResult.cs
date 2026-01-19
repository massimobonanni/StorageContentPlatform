namespace StorageContentPlatform.ManagementFunctions.Entities
{
    /// <summary>
    /// Represents the result of a manifest processing operation.
    /// </summary>
    public class ManifestProcessingResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the processing was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets an error message if processing failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the inventory statistics if processing succeeded.
        /// </summary>
        public InventoryStatistics Statistics { get; set; }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static ManifestProcessingResult CreateSuccess(InventoryStatistics statistics)
        {
            return new ManifestProcessingResult
            {
                Success = true,
                Statistics = statistics
            };
        }

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        public static ManifestProcessingResult CreateFailure(string errorMessage)
        {
            return new ManifestProcessingResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}