using System.Runtime.InteropServices;

namespace SharedUtilities.FileSystem;

/// <summary>
/// Cross-platform file system manager with home directory resolution
/// </summary>
public class FileSystemManager : IFileSystemManager
{
    private readonly string _rootDirectory;
    
    public FileSystemManager()
    {
        // Determine root directory based on platform
        _rootDirectory = DetermineRootDirectory();
        
        // Ensure root directory exists on first launch
        EnsureRootDirectoryExists();
    }
    
    private string DetermineRootDirectory()
    {
        // On Android: /storage/emulated/0/root/
        // On other platforms: use a local directory for development/testing
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && IsAndroid())
        {
            return "/storage/emulated/0/root/";
        }
        
        // For Windows/Linux/macOS development, use a local directory
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(homeDir, ".android-dev-ecosystem", "root");
    }
    
    private bool IsAndroid()
    {
        // Check if running on Android by looking for Android-specific environment
        return Directory.Exists("/system/app") && Directory.Exists("/data/data");
    }
    
    public string GetRootDirectory() => _rootDirectory;
    
    public string ResolvePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return _rootDirectory;
        
        // Handle ~ (home directory) expansion
        if (path.StartsWith("~"))
        {
            if (path.Length == 1)
                return _rootDirectory;
            
            // Replace ~ with root directory
            var relativePath = path.Substring(1).TrimStart('/', '\\');
            return Path.Combine(_rootDirectory, relativePath);
        }
        
        // If already absolute, return as-is
        if (Path.IsPathRooted(path))
            return path;
        
        // Otherwise, make it relative to root directory
        return Path.Combine(_rootDirectory, path);
    }
    
    public bool FileExists(string path)
    {
        var resolvedPath = ResolvePath(path);
        return File.Exists(resolvedPath);
    }
    
    public bool DirectoryExists(string path)
    {
        var resolvedPath = ResolvePath(path);
        return Directory.Exists(resolvedPath);
    }
    
    public string ReadAllText(string path)
    {
        var resolvedPath = ResolvePath(path);
        return File.ReadAllText(resolvedPath);
    }
    
    public void WriteAllText(string path, string content)
    {
        var resolvedPath = ResolvePath(path);
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        File.WriteAllText(resolvedPath, content);
    }
    
    public void CreateDirectory(string path)
    {
        var resolvedPath = ResolvePath(path);
        Directory.CreateDirectory(resolvedPath);
    }
    
    public void DeleteFile(string path)
    {
        var resolvedPath = ResolvePath(path);
        if (File.Exists(resolvedPath))
        {
            File.Delete(resolvedPath);
        }
    }
    
    public void DeleteDirectory(string path, bool recursive = false)
    {
        var resolvedPath = ResolvePath(path);
        if (Directory.Exists(resolvedPath))
        {
            Directory.Delete(resolvedPath, recursive);
        }
    }
    
    public string[] GetFiles(string path, string searchPattern = "*", bool recursive = false)
    {
        var resolvedPath = ResolvePath(path);
        if (!Directory.Exists(resolvedPath))
            return Array.Empty<string>();
        
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return Directory.GetFiles(resolvedPath, searchPattern, searchOption);
    }
    
    public string[] GetDirectories(string path)
    {
        var resolvedPath = ResolvePath(path);
        if (!Directory.Exists(resolvedPath))
            return Array.Empty<string>();
        
        return Directory.GetDirectories(resolvedPath);
    }
    
    public string CombinePath(params string[] paths)
    {
        return Path.Combine(paths);
    }
    
    public string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }
    
    public string GetDirectoryName(string path)
    {
        return Path.GetDirectoryName(path) ?? string.Empty;
    }
    
    public void EnsureRootDirectoryExists()
    {
        if (!Directory.Exists(_rootDirectory))
        {
            Directory.CreateDirectory(_rootDirectory);
        }
    }
    
    public void CopyFile(string sourcePath, string destinationPath, bool overwrite = false)
    {
        var resolvedSource = ResolvePath(sourcePath);
        var resolvedDestination = ResolvePath(destinationPath);
        
        // Ensure destination directory exists
        var destDir = Path.GetDirectoryName(resolvedDestination);
        if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }
        
        File.Copy(resolvedSource, resolvedDestination, overwrite);
    }
    
    public void CopyDirectory(string sourcePath, string destinationPath, bool recursive = true)
    {
        var resolvedSource = ResolvePath(sourcePath);
        var resolvedDestination = ResolvePath(destinationPath);
        
        if (!Directory.Exists(resolvedSource))
        {
            throw new DirectoryNotFoundException($"Source directory not found: {resolvedSource}");
        }
        
        // Create destination directory
        Directory.CreateDirectory(resolvedDestination);
        
        // Copy all files
        foreach (var file in Directory.GetFiles(resolvedSource))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(resolvedDestination, fileName);
            File.Copy(file, destFile, overwrite: true);
        }
        
        // Copy subdirectories recursively if requested
        if (recursive)
        {
            foreach (var directory in Directory.GetDirectories(resolvedSource))
            {
                var dirName = Path.GetFileName(directory);
                var destDir = Path.Combine(resolvedDestination, dirName);
                CopyDirectory(directory, destDir, recursive: true);
            }
        }
    }
    
    public void MoveFile(string sourcePath, string destinationPath)
    {
        var resolvedSource = ResolvePath(sourcePath);
        var resolvedDestination = ResolvePath(destinationPath);
        
        // Ensure destination directory exists
        var destDir = Path.GetDirectoryName(resolvedDestination);
        if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }
        
        File.Move(resolvedSource, resolvedDestination);
    }
    
    public void MoveDirectory(string sourcePath, string destinationPath)
    {
        var resolvedSource = ResolvePath(sourcePath);
        var resolvedDestination = ResolvePath(destinationPath);
        
        // Ensure parent directory of destination exists
        var destParent = Path.GetDirectoryName(resolvedDestination);
        if (!string.IsNullOrEmpty(destParent) && !Directory.Exists(destParent))
        {
            Directory.CreateDirectory(destParent);
        }
        
        Directory.Move(resolvedSource, resolvedDestination);
    }
    
    public IFileSystemWatcher CreateWatcher(string path, string filter = "*.*")
    {
        var resolvedPath = ResolvePath(path);
        
        // Ensure directory exists before creating watcher
        if (!Directory.Exists(resolvedPath))
        {
            throw new DirectoryNotFoundException($"Cannot watch non-existent directory: {resolvedPath}");
        }
        
        return new FileSystemWatcher(resolvedPath, filter);
    }
}
