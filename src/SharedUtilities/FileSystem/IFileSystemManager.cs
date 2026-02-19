namespace SharedUtilities.FileSystem;

/// <summary>
/// Interface for cross-platform file system operations
/// </summary>
public interface IFileSystemManager
{
    /// <summary>
    /// Resolves a path, handling ~ (home directory) expansion
    /// </summary>
    string ResolvePath(string path);
    
    /// <summary>
    /// Gets the root directory path (/storage/emulated/0/root/)
    /// </summary>
    string GetRootDirectory();
    
    /// <summary>
    /// Checks if a file exists
    /// </summary>
    bool FileExists(string path);
    
    /// <summary>
    /// Checks if a directory exists
    /// </summary>
    bool DirectoryExists(string path);
    
    /// <summary>
    /// Reads all text from a file
    /// </summary>
    string ReadAllText(string path);
    
    /// <summary>
    /// Writes text to a file
    /// </summary>
    void WriteAllText(string path, string content);
    
    /// <summary>
    /// Creates a directory
    /// </summary>
    void CreateDirectory(string path);
    
    /// <summary>
    /// Deletes a file
    /// </summary>
    void DeleteFile(string path);
    
    /// <summary>
    /// Deletes a directory recursively
    /// </summary>
    void DeleteDirectory(string path, bool recursive = false);
    
    /// <summary>
    /// Gets all files in a directory
    /// </summary>
    string[] GetFiles(string path, string searchPattern = "*", bool recursive = false);
    
    /// <summary>
    /// Gets all directories in a directory
    /// </summary>
    string[] GetDirectories(string path);
    
    /// <summary>
    /// Combines path segments
    /// </summary>
    string CombinePath(params string[] paths);
    
    /// <summary>
    /// Gets the file name from a path
    /// </summary>
    string GetFileName(string path);
    
    /// <summary>
    /// Gets the directory name from a path
    /// </summary>
    string GetDirectoryName(string path);
    
    /// <summary>
    /// Ensures the root directory exists, creating it if necessary
    /// </summary>
    void EnsureRootDirectoryExists();
    
    /// <summary>
    /// Copies a file from source to destination
    /// </summary>
    void CopyFile(string sourcePath, string destinationPath, bool overwrite = false);
    
    /// <summary>
    /// Copies a directory recursively
    /// </summary>
    void CopyDirectory(string sourcePath, string destinationPath, bool recursive = true);
    
    /// <summary>
    /// Moves a file from source to destination
    /// </summary>
    void MoveFile(string sourcePath, string destinationPath);
    
    /// <summary>
    /// Moves a directory from source to destination
    /// </summary>
    void MoveDirectory(string sourcePath, string destinationPath);
    
    /// <summary>
    /// Watches a file or directory for changes
    /// </summary>
    IFileSystemWatcher CreateWatcher(string path, string filter = "*.*");
}
