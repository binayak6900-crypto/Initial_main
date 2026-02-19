namespace ADLCompiler;

/// <summary>
/// Parses command-line arguments for the ADL compiler.
/// </summary>
public class CommandLineParser
{
    /// <summary>
    /// Parse command-line arguments into structured options.
    /// </summary>
    public static CommandLineOptions Parse(string[] args)
    {
        var options = new CommandLineOptions();
        
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            
            switch (arg)
            {
                case "-C":
                    options.Compile = true;
                    break;
                    
                case "-E":
                    options.PreprocessOnly = true;
                    break;
                    
                case "-o":
                    if (i + 1 < args.Length)
                    {
                        options.OutputPath = args[++i];
                    }
                    else
                    {
                        throw new ArgumentException("Missing output path after -o flag");
                    }
                    break;
                    
                case "--platform":
                    if (i + 1 < args.Length)
                    {
                        options.Platform = ParsePlatform(args[++i]);
                    }
                    else
                    {
                        throw new ArgumentException("Missing platform after --platform flag");
                    }
                    break;
                    
                case "--package-mode":
                    if (i + 1 < args.Length)
                    {
                        options.PackageMode = ParsePackageMode(args[++i]);
                    }
                    else
                    {
                        throw new ArgumentException("Missing mode after --package-mode flag");
                    }
                    break;
                    
                case "--target-sdk":
                    if (i + 1 < args.Length)
                    {
                        if (!int.TryParse(args[++i], out int targetSdk))
                        {
                            throw new ArgumentException($"Invalid target SDK version: {args[i]}");
                        }
                        options.TargetSdk = targetSdk;
                    }
                    else
                    {
                        throw new ArgumentException("Missing SDK version after --target-sdk flag");
                    }
                    break;
                    
                case "--min-sdk":
                    if (i + 1 < args.Length)
                    {
                        if (!int.TryParse(args[++i], out int minSdk))
                        {
                            throw new ArgumentException($"Invalid minimum SDK version: {args[i]}");
                        }
                        options.MinSdk = minSdk;
                    }
                    else
                    {
                        throw new ArgumentException("Missing SDK version after --min-sdk flag");
                    }
                    break;
                    
                case "-O0":
                    options.OptimizationLevel = 0;
                    break;
                    
                case "-O1":
                    options.OptimizationLevel = 1;
                    break;
                    
                case "-O2":
                    options.OptimizationLevel = 2;
                    break;
                    
                case "-O3":
                    options.OptimizationLevel = 3;
                    break;
                    
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    break;
                    
                case "-v":
                case "--version":
                    options.ShowVersion = true;
                    break;
                    
                default:
                    // If it doesn't start with -, treat it as source file
                    if (!arg.StartsWith("-"))
                    {
                        if (options.SourceFile == null)
                        {
                            options.SourceFile = arg;
                        }
                        else
                        {
                            throw new ArgumentException($"Multiple source files specified: {options.SourceFile} and {arg}");
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown option: {arg}");
                    }
                    break;
            }
        }
        
        return options;
    }
    
    private static TargetPlatform ParsePlatform(string platform)
    {
        return platform.ToLowerInvariant() switch
        {
            "android" => TargetPlatform.Android,
            "windows" => TargetPlatform.Windows,
            "linux" => TargetPlatform.Linux,
            "macos" => TargetPlatform.MacOS,
            "all" => TargetPlatform.All,
            _ => throw new ArgumentException($"Unknown platform: {platform}. Valid options: android, windows, linux, macos, all")
        };
    }
    
    private static PackageMode ParsePackageMode(string mode)
    {
        return mode.ToLowerInvariant() switch
        {
            "installer" => PackageMode.Installer,
            "standalone" => PackageMode.Standalone,
            _ => throw new ArgumentException($"Unknown package mode: {mode}. Valid options: installer, standalone")
        };
    }
    
    /// <summary>
    /// Validate parsed options and apply zero-configuration defaults.
    /// </summary>
    public static void ValidateAndApplyDefaults(CommandLineOptions options)
    {
        // If no mode specified, default to compile
        if (!options.Compile && !options.PreprocessOnly && !options.ShowHelp && !options.ShowVersion)
        {
            options.Compile = true;
        }
        
        // If compiling but no source file, error
        if ((options.Compile || options.PreprocessOnly) && string.IsNullOrEmpty(options.SourceFile))
        {
            throw new ArgumentException("No source file specified");
        }
        
        // Apply default output path if not specified
        if ((options.Compile || options.PreprocessOnly) && string.IsNullOrEmpty(options.OutputPath))
        {
            options.OutputPath = GenerateDefaultOutputPath(options);
        }
        
        // Validate SDK versions
        if (options.MinSdk < 23)
        {
            throw new ArgumentException("Minimum SDK version must be at least 23 (Android 6.0)");
        }
        
        if (options.TargetSdk < options.MinSdk)
        {
            throw new ArgumentException($"Target SDK ({options.TargetSdk}) cannot be lower than minimum SDK ({options.MinSdk})");
        }
    }
    
    private static string GenerateDefaultOutputPath(CommandLineOptions options)
    {
        if (options.PreprocessOnly)
        {
            // Preprocessed output: source.adl -> source.i
            return Path.ChangeExtension(options.SourceFile, ".i");
        }
        
        // Compilation output depends on platform
        string baseName = Path.GetFileNameWithoutExtension(options.SourceFile);
        
        return options.Platform switch
        {
            TargetPlatform.Android => $"{baseName}.apk",
            TargetPlatform.Windows => $"{baseName}.exe",
            TargetPlatform.Linux => baseName,
            TargetPlatform.MacOS => $"{baseName}.app",
            TargetPlatform.All => $"{baseName}_all", // Will generate multiple files
            _ => $"{baseName}.out"
        };
    }
}
