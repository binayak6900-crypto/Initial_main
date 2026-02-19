using System.Text;
using System.Text.RegularExpressions;

namespace ADLCompiler.Preprocessing;

/// <summary>
/// Preprocessor for ADL language supporting C-style directives
/// </summary>
public class Preprocessor
{
    private readonly PreprocessorState _state = new();
    private readonly LineMapping _lineMapping = new();
    private int _outputLine = 1;
    
    /// <summary>
    /// Process source code with preprocessing directives
    /// </summary>
    public PreprocessedSource Process(string source, string fileName)
    {
        _state.CurrentFile = fileName;
        _state.CurrentLine = 1;
        _outputLine = 1;
        
        var lines = source.Split('\n');
        var output = new StringBuilder();
        
        foreach (var line in lines)
        {
            string processedLine = ProcessLine(line.TrimEnd('\r'));
            
            if (!string.IsNullOrEmpty(processedLine))
            {
                output.AppendLine(processedLine);
                _lineMapping.AddMapping(_outputLine, _state.CurrentFile, _state.CurrentLine);
                _outputLine++;
            }
            
            _state.CurrentLine++;
        }
        
        // Check for unclosed conditional blocks
        if (_state.ConditionalStack.Count > 0)
        {
            var unclosed = _state.ConditionalStack.Peek();
            throw new PreprocessorException(
                $"Unclosed {unclosed.Directive} directive for symbol '{unclosed.Symbol}'",
                fileName,
                _state.CurrentLine);
        }
        
        return new PreprocessedSource
        {
            Source = output.ToString(),
            LineMapping = _lineMapping
        };
    }
    
    /// <summary>
    /// Process a single line
    /// </summary>
    private string ProcessLine(string line)
    {
        string trimmed = line.TrimStart();
        
        // Check if this is a preprocessor directive
        if (trimmed.StartsWith("#"))
        {
            ProcessDirective(trimmed);
            return string.Empty; // Directives are not included in output
        }
        
        // If we're in an inactive block, skip this line
        if (!_state.IsActive)
        {
            return string.Empty;
        }
        
        // Expand macros in the line
        return ExpandMacros(line);
    }
    
    /// <summary>
    /// Process a preprocessor directive
    /// </summary>
    private void ProcessDirective(string line)
    {
        // Remove the # and split into tokens
        string directive = line.Substring(1).Trim();
        var tokens = Regex.Split(directive, @"\s+", RegexOptions.None, TimeSpan.FromSeconds(1));
        
        if (tokens.Length == 0)
        {
            return;
        }
        
        string command = tokens[0].ToLower();
        
        switch (command)
        {
            case "define":
                // Process #define even inside conditionals, but only if the block is active
                if (_state.IsActive)
                {
                    ProcessDefine(directive.Substring(6).Trim());
                }
                break;
                
            case "ifdef":
                ProcessIfdef(tokens);
                break;
                
            case "ifndef":
                ProcessIfndef(tokens);
                break;
                
            case "endif":
                ProcessEndif();
                break;
                
            case "else":
                ProcessElse();
                break;
                
            default:
                // Ignore unknown directives
                break;
        }
    }
    
    /// <summary>
    /// Process #define directive
    /// </summary>
    private void ProcessDefine(string definition)
    {
        // Parse: MACRO_NAME or MACRO_NAME(params) replacement
        var match = Regex.Match(definition, 
            @"^(\w+)(?:\(([^)]*)\))?\s*(.*?)$",
            RegexOptions.None,
            TimeSpan.FromSeconds(1));
        
        if (!match.Success)
        {
            throw new PreprocessorException(
                $"Invalid #define syntax: {definition}",
                _state.CurrentFile,
                _state.CurrentLine);
        }
        
        string name = match.Groups[1].Value;
        string paramsStr = match.Groups[2].Value;
        string replacement = match.Groups[3].Value;
        
        List<string>? parameters = null;
        if (!string.IsNullOrEmpty(paramsStr))
        {
            // Function-like macro
            parameters = paramsStr.Split(',')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();
        }
        
        _state.Defines[name] = new MacroDefinition
        {
            Name = name,
            Parameters = parameters,
            Replacement = replacement
        };
    }
    
    /// <summary>
    /// Process #ifdef directive
    /// </summary>
    private void ProcessIfdef(string[] tokens)
    {
        if (tokens.Length < 2)
        {
            throw new PreprocessorException(
                "#ifdef requires a symbol name",
                _state.CurrentFile,
                _state.CurrentLine);
        }
        
        string symbol = tokens[1];
        bool isDefined = _state.Defines.ContainsKey(symbol);
        
        // Only activate if parent block is active
        bool isActive = _state.IsActive && isDefined;
        
        _state.ConditionalStack.Push(new ConditionalState
        {
            IsActive = isActive,
            Directive = "#ifdef",
            Symbol = symbol
        });
    }
    
    /// <summary>
    /// Process #ifndef directive
    /// </summary>
    private void ProcessIfndef(string[] tokens)
    {
        if (tokens.Length < 2)
        {
            throw new PreprocessorException(
                "#ifndef requires a symbol name",
                _state.CurrentFile,
                _state.CurrentLine);
        }
        
        string symbol = tokens[1];
        bool isDefined = _state.Defines.ContainsKey(symbol);
        
        // Only activate if parent block is active and symbol is NOT defined
        bool isActive = _state.IsActive && !isDefined;
        
        _state.ConditionalStack.Push(new ConditionalState
        {
            IsActive = isActive,
            Directive = "#ifndef",
            Symbol = symbol
        });
    }
    
    /// <summary>
    /// Process #endif directive
    /// </summary>
    private void ProcessEndif()
    {
        if (_state.ConditionalStack.Count == 0)
        {
            throw new PreprocessorException(
                "#endif without matching #ifdef or #ifndef",
                _state.CurrentFile,
                _state.CurrentLine);
        }
        
        _state.ConditionalStack.Pop();
    }
    
    /// <summary>
    /// Process #else directive
    /// </summary>
    private void ProcessElse()
    {
        if (_state.ConditionalStack.Count == 0)
        {
            throw new PreprocessorException(
                "#else without matching #ifdef or #ifndef",
                _state.CurrentFile,
                _state.CurrentLine);
        }
        
        var current = _state.ConditionalStack.Pop();
        
        // Flip the active state (but only if parent is active)
        bool parentActive = _state.ConditionalStack.Count == 0 || _state.ConditionalStack.All(s => s.IsActive);
        current.IsActive = parentActive && !current.IsActive;
        
        _state.ConditionalStack.Push(current);
    }
    
    /// <summary>
    /// Expand macros in a line of code
    /// </summary>
    private string ExpandMacros(string line)
    {
        string result = line;
        bool changed;
        int maxIterations = 100; // Prevent infinite recursion
        int iterations = 0;
        
        do
        {
            changed = false;
            iterations++;
            
            if (iterations > maxIterations)
            {
                throw new PreprocessorException(
                    "Macro expansion exceeded maximum iterations (possible recursive macro)",
                    _state.CurrentFile,
                    _state.CurrentLine);
            }
            
            foreach (var macro in _state.Defines.Values)
            {
                if (macro.IsFunctionLike)
                {
                    // Function-like macro: MACRO(args)
                    // We need to find the macro name followed by parentheses and extract the arguments
                    string pattern = $@"\b{Regex.Escape(macro.Name)}\s*\(";
                    var matches = Regex.Matches(result, pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
                    
                    // Process matches in reverse order to avoid index shifting
                    for (int i = matches.Count - 1; i >= 0; i--)
                    {
                        var match = matches[i];
                        int startPos = match.Index;
                        int parenStart = match.Index + match.Length - 1; // Position of opening (
                        
                        // Find the matching closing parenthesis
                        int parenEnd = FindMatchingParen(result, parenStart);
                        if (parenEnd == -1)
                        {
                            continue; // Malformed, skip
                        }
                        
                        // Extract arguments
                        string argsStr = result.Substring(parenStart + 1, parenEnd - parenStart - 1);
                        var args = ParseMacroArguments(argsStr);
                        
                        try
                        {
                            string expansion = macro.Expand(args);
                            string fullMatch = result.Substring(startPos, parenEnd - startPos + 1);
                            result = result.Substring(0, startPos) + expansion + result.Substring(parenEnd + 1);
                            changed = true;
                        }
                        catch (InvalidOperationException ex)
                        {
                            throw new PreprocessorException(
                                ex.Message,
                                _state.CurrentFile,
                                _state.CurrentLine);
                        }
                    }
                }
                else
                {
                    // Object-like macro: simple replacement
                    var pattern = $@"\b{Regex.Escape(macro.Name)}\b";
                    if (Regex.IsMatch(result, pattern, RegexOptions.None, TimeSpan.FromSeconds(1)))
                    {
                        result = Regex.Replace(result, pattern, macro.Replacement, RegexOptions.None, TimeSpan.FromSeconds(1));
                        changed = true;
                    }
                }
            }
        } while (changed);
        
        return result;
    }
    
    /// <summary>
    /// Find the matching closing parenthesis for an opening parenthesis
    /// </summary>
    private int FindMatchingParen(string text, int openPos)
    {
        int depth = 1;
        for (int i = openPos + 1; i < text.Length; i++)
        {
            if (text[i] == '(') depth++;
            else if (text[i] == ')') depth--;
            
            if (depth == 0)
            {
                return i;
            }
        }
        return -1; // No matching paren found
    }
    
    /// <summary>
    /// Parse macro arguments from a comma-separated string
    /// </summary>
    private List<string> ParseMacroArguments(string argsStr)
    {
        if (string.IsNullOrWhiteSpace(argsStr))
        {
            return new List<string>();
        }
        
        var args = new List<string>();
        var currentArg = new StringBuilder();
        int parenDepth = 0;
        int bracketDepth = 0;
        
        foreach (char c in argsStr)
        {
            if (c == ',' && parenDepth == 0 && bracketDepth == 0)
            {
                args.Add(currentArg.ToString().Trim());
                currentArg.Clear();
            }
            else
            {
                if (c == '(') parenDepth++;
                if (c == ')') parenDepth--;
                if (c == '[') bracketDepth++;
                if (c == ']') bracketDepth--;
                currentArg.Append(c);
            }
        }
        
        if (currentArg.Length > 0)
        {
            args.Add(currentArg.ToString().Trim());
        }
        
        return args;
    }
}

/// <summary>
/// Result of preprocessing
/// </summary>
public class PreprocessedSource
{
    public string Source { get; set; } = string.Empty;
    public LineMapping LineMapping { get; set; } = new();
}

/// <summary>
/// Exception thrown during preprocessing
/// </summary>
public class PreprocessorException : Exception
{
    public string FileName { get; }
    public int LineNumber { get; }
    
    public PreprocessorException(string message, string fileName, int lineNumber)
        : base($"{fileName}:{lineNumber}: {message}")
    {
        FileName = fileName;
        LineNumber = lineNumber;
    }
}
