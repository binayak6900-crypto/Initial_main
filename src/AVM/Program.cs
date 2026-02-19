using SharedUtilities.FileSystem;

namespace AVM;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Android Virtual Machine (AVM) - Version 1.0");
        
        var fileSystem = new FileSystemManager();
        Console.WriteLine($"Root Directory: {fileSystem.GetRootDirectory()}");
        
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: avm <apk-file>");
            Console.WriteLine("Executes an Android APK file.");
            return;
        }
        
        Console.WriteLine("AVM ready.");
    }
}
