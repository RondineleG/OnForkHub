namespace OnForkHub.CrossCutting.Storage;

/// <summary>
/// Configuration options for Azure Blob Storage.
/// </summary>
public sealed class AzureBlobStorageOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "AzureBlobStorage";

    /// <summary>
    /// Gets or sets the Azure storage connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the container.
    /// </summary>
    public string ContainerName { get; set; } = "videos";
}
