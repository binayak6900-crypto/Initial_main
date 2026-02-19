using SharedUtilities.FileSystem;

namespace AndroidIDE;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Android IDE - Starting...");
        
        var fileSystem = new FileSystemManager();
        var rootDir = fileSystem.GetRootDirectory();
        
        Console.WriteLine($"Root Directory: {rootDir}");
        
        // Ensure root directory exists
        if (!fileSystem.DirectoryExists(rootDir))
        {
            fileSystem.CreateDirectory(rootDir);
            Console.WriteLine("Created root directory.");
        }
        
        Console.WriteLine("Android IDE initialized successfully.");
    }
}
