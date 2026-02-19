namespace SharedUtilities.FileSystem;

/// <summary>
/// Wrapper around System.IO.FileSystemWatcher for file change detection
/// </summary>
public class FileSystemWatcher : IFileSystemWatcher
{
    private readonly System.IO.FileSystemWatcher _watcher;
    private bool _disposed;
    
    public event EventHandler<FileSystemEventArgs>? Created;
    public event EventHandler<FileSystemEventArgs>? Changed;
    public event EventHandler<FileSystemEventArgs>? Deleted;
    public event EventHandler<FileSystemRenamedEventArgs>? Renamed;
    
    public bool EnableRaisingEvents
    {
        get => _watcher.EnableRaisingEvents;
        set => _watcher.EnableRaisingEvents = value;
    }
    
    public string Path => _watcher.Path;
    public string Filter => _watcher.Filter;
    
    public FileSystemWatcher(string path, string filter = "*.*")
    {
        _watcher = new System.IO.FileSystemWatcher(path, filter)
        {
            NotifyFilter = NotifyFilters.FileName 
                         | NotifyFilters.DirectoryName 
                         | NotifyFilters.LastWrite 
                         | NotifyFilters.Size
        };
        
        // Wire up events
        _watcher.Created += OnCreated;
        _watcher.Changed += OnChanged;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
    }
    
    private void OnCreated(object sender, System.IO.FileSystemEventArgs e)
    {
        Created?.Invoke(this, new FileSystemEventArgs(
            FileSystemChangeType.Created, 
            e.FullPath, 
            e.Name ?? string.Empty));
    }
    
    private void OnChanged(object sender, System.IO.FileSystemEventArgs e)
    {
        Changed?.Invoke(this, new FileSystemEventArgs(
            FileSystemChangeType.Changed, 
            e.FullPath, 
            e.Name ?? string.Empty));
    }
    
    private void OnDeleted(object sender, System.IO.FileSystemEventArgs e)
    {
        Deleted?.Invoke(this, new FileSystemEventArgs(
            FileSystemChangeType.Deleted, 
            e.FullPath, 
            e.Name ?? string.Empty));
    }
    
    private void OnRenamed(object sender, System.IO.RenamedEventArgs e)
    {
        Renamed?.Invoke(this, new FileSystemRenamedEventArgs(
            e.FullPath, 
            e.Name ?? string.Empty,
            e.OldFullPath,
            e.OldName ?? string.Empty));
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        _watcher.EnableRaisingEvents = false;
        _watcher.Created -= OnCreated;
        _watcher.Changed -= OnChanged;
        _watcher.Deleted -= OnDeleted;
        _watcher.Renamed -= OnRenamed;
        _watcher.Dispose();
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
