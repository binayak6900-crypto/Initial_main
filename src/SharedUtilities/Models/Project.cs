namespace SharedUtilities.Models;

/// <summary>
/// Represents an ADL project containing multiple files and build configuration
/// </summary>
public class Project
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public List<ADLFile> Files { get; set; } = new();
    public BuildConfiguration BuildConfig { get; set; } = new();
    public string TargetSDK { get; set; } = "15.0";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
