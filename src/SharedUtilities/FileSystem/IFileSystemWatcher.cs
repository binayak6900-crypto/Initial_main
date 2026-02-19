namespace SharedUtilities.FileSystem;

/// <summary>
/// Interface for file system change notifications
/// </summary>
public interface IFileSystemWatcher : IDisposable
{
    /// <summary>
    /// Event raised when a file or directory is created
    /// </summary>
    event EventHandler<FileSystemEventArgs>? Created;
    
    /// <summary>
    /// Event raised when a file or directory is changed
    /// </summary>
    event EventHandler<FileSystemEventArgs>? Changed;
    
    /// <summary>
    /// Event raised when a file or directory is deleted
    /// </summary>
    event EventHandler<FileSystemEventArgs>? Deleted;
    
    /// <summary>
    /// Event raised when a file or directory is renamed
    /// </summary>
    event EventHandler<FileSystemRenamedEventArgs>? Renamed;
    
    /// <summary>
    /// Gets or sets whether the watcher is enabled
    /// </summary>
    bool EnableRaisingEvents { get; set; }
    
    /// <summary>
    /// Gets the path being watched
    /// </summary>
    string Path { get; }
    
    /// <summary>
    /// Gets the filter pattern
    /// </summary>
    string Filter { get; }
}

/// <summary>
/// Event arguments for file system changes
/// </summary>
public class FileSystemEventArgs : EventArgs
{
    public string FullPath { get; }
    public string Name { get; }
    public FileSystemChangeType ChangeType { get; }
    
    public FileSystemEventArgs(FileSystemChangeType changeType, string fullPath, string name)
    {
        ChangeType = changeType;
        FullPath = fullPath;
        Name = name;
    }
}

/// <summary>
/// Event arguments for file system rename operations
/// </summary>
public class FileSystemRenamedEventArgs : FileSystemEventArgs
{
    public string OldFullPath { get; }
    public string OldName { get; }
    
    public FileSystemRenamedEventArgs(string fullPath, string name, string oldFullPath, string oldName)
        : base(FileSystemChangeType.Renamed, fullPath, name)
    {
        OldFullPath = oldFullPath;
        OldName = oldName;
    }
}

/// <summary>
/// Types of file system changes
/// </summary>
public enum FileSystemChangeType
{
    Created,
    Changed,
    Deleted,
    Renamed
}
