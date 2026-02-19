namespace SharedUtilities.Models;

/// <summary>
/// Represents a single ADL source code file
/// </summary>
public class ADLFile
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public FileStatus Status { get; set; } = FileStatus.Unmodified;
}

public enum FileStatus
{
    Unmodified,
    Modified,
    New,
    Deleted
}
