namespace ADLCompiler;

class Program
{
    static int Main(string[] args)
    {
        // Show version banner
        Console.WriteLine("ADL Compiler - Version 1.0.0");
        Console.WriteLine("Zero-configuration compiler for Android Development Language");
        Console.WriteLine();
        
        // Handle no arguments - show help
        if (args.Length == 0)
        {
            ShowHelp();
            return 0;
        }
        
        try
        {
            // Parse command-line arguments
            var options = CommandLineParser.Parse(args);
            
            // Handle special flags
            if (options.ShowHelp)
            {
                ShowHelp();
                return 0;
            }
            
            if (options.ShowVersion)
            {
                ShowVersion();
                return 0;
            }
            
            // Validate and apply defaults
            CommandLineParser.ValidateAndApplyDefaults(options);
            
            // Execute compilation
            var driver = new CompilerDriver();
            return driver.Execute(options);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Run 'adlc --help' for usage information.");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }
    
    static void ShowHelp()
    {
        Console.WriteLine("USAGE:");
        Console.WriteLine("  adlc [options] <source-file>");
        Console.WriteLine();
        Console.WriteLine("DESCRIPTION:");
        Console.WriteLine("  Compile ADL source code to executable format with zero configuration.");
        Console.WriteLine("  Just specify the entry file - the compiler handles everything else!");
        Console.WriteLine();
        Console.WriteLine("OPTIONS:");
        Console.WriteLine("  -C                      Compile to executable (default if no mode specified)");
        Console.WriteLine("  -E                      Preprocess only - output preprocessed source");
        Console.WriteLine("  -o <file>               Output file path (auto-generated if not specified)");
        Console.WriteLine();
        Console.WriteLine("  --platform <platform>   Target platform (default: android)");
        Console.WriteLine("                          Options: android, windows, linux, macos, all");
        Console.WriteLine();
        Console.WriteLine("  --package-mode <mode>   APK packaging mode (default: standalone)");
        Console.WriteLine("                          Options:");
        Console.WriteLine("                            installer   - Installs AVM runtime and tools");
        Console.WriteLine("                            standalone  - No external dependencies");
        Console.WriteLine();
        Console.WriteLine("  --target-sdk <version>  Target Android SDK API level (default: 35)");
        Console.WriteLine("  --min-sdk <version>     Minimum Android SDK API level (default: 23)");
        Console.WriteLine();
        Console.WriteLine("  -O0, -O1, -O2, -O3      Optimization level (default: -O2)");
        Console.WriteLine();
        Console.WriteLine("  -h, --help              Show this help message");
        Console.WriteLine("  -v, --version           Show version information");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES:");
        Console.WriteLine("  # Simple compilation - just specify the file!");
        Console.WriteLine("  adlc Main.adl");
        Console.WriteLine();
        Console.WriteLine("  # Compile to specific output");
        Console.WriteLine("  adlc -C -o MyApp.apk Main.adl");
        Console.WriteLine();
        Console.WriteLine("  # Cross-compile to Windows from Android");
        Console.WriteLine("  adlc --platform windows -o MyApp.exe Main.adl");
        Console.WriteLine();
        Console.WriteLine("  # Compile for all platforms at once");
        Console.WriteLine("  adlc --platform all Main.adl");
        Console.WriteLine();
        Console.WriteLine("  # Create installer APK (includes runtime)");
        Console.WriteLine("  adlc --package-mode installer -o MyApp.apk Main.adl");
        Console.WriteLine();
        Console.WriteLine("  # Preprocess only (for debugging)");
        Console.WriteLine("  adlc -E -o Main.i Main.adl");
        Console.WriteLine();
        Console.WriteLine("ZERO CONFIGURATION:");
        Console.WriteLine("  The compiler uses sensible defaults for everything:");
        Console.WriteLine("  • Latest Android SDK (API 35)");
        Console.WriteLine("  • Optimized for current device");
        Console.WriteLine("  • Automatic dependency discovery");
        Console.WriteLine("  • Automatic memory management");
        Console.WriteLine("  • No config files needed!");
        Console.WriteLine();
        Console.WriteLine("For more information, visit: https://adl-lang.org");
    }
    
    static void ShowVersion()
    {
        Console.WriteLine("ADL Compiler Version 1.0.0");
        Console.WriteLine("Build Date: 2024-01-15");
        Console.WriteLine();
        Console.WriteLine("Features:");
        Console.WriteLine("  • Zero-configuration compilation");
        Console.WriteLine("  • Cross-platform support (Android, Windows, Linux, macOS)");
        Console.WriteLine("  • Automatic dependency discovery");
        Console.WriteLine("  • Automatic memory management");
        Console.WriteLine("  • Built-in Android SDK, NDK, and raylib APIs");
        Console.WriteLine("  • No external dependencies required");
        Console.WriteLine();
        Console.WriteLine("Supported SDK Versions: Android 6.0 (API 23) - 15.0 (API 35)");
        Console.WriteLine("Supported Platforms: Android, Windows, Linux, macOS");
        Console.WriteLine();
        Console.WriteLine("Copyright (c) 2024 ADL Project");
        Console.WriteLine("License: MIT");
    }
}
