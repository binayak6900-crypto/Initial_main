using SharedUtilities.FileSystem;

namespace Shell;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Android Development Shell - Version 1.0");
        
        var fileSystem = new FileSystemManager();
        var rootDir = fileSystem.GetRootDirectory();
        
        Console.WriteLine($"Root Directory: {rootDir}");
        
        // Ensure root directory exists
        if (!fileSystem.DirectoryExists(rootDir))
        {
            fileSystem.CreateDirectory(rootDir);
            Console.WriteLine("Created root directory.");
        }
        
        Console.WriteLine("Shell initialized. Type 'exit' to quit.");
        Console.WriteLine();
        
        // Simple shell loop
        while (true)
        {
            Console.Write($"{rootDir}> ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(input))
                continue;
            
            if (input.Trim().ToLower() == "exit")
                break;
            
            Console.WriteLine($"Command not yet implemented: {input}");
        }
        
        Console.WriteLine("Shell terminated.");
    }
}
