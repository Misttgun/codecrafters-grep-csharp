namespace codecrafters_grep;

public static class Parser
{
    public readonly record struct Options(string Pattern, List<string> Paths, bool PrintMatchesOnly, bool RecursiveSearch);
    
    public static bool TryParseArgs(string[] args, out Options options)
    {
        options = default;
        
        // Required by the challenge (at least for now).
        if (args.Contains("-E") == false)
        {
            Console.WriteLine("Expected arguments to contain '-E'");
            return false;
        }

        var printMatchesOnly = args.Contains("-o");
        var recursiveSearch = args.Contains("-r");

        // First non-flag token is the pattern.
        string? pattern = null;
        List<string> paths = new List<string>();
        foreach (var arg in args)
        {
            if (arg.Length > 0 && arg.StartsWith('-') == false)
            {
                if (pattern == null)
                    pattern = arg;
                else
                    paths.Add(arg);
            }
        }
        
        if (string.IsNullOrEmpty(pattern))
        {
            Console.WriteLine("Expected a pattern argument");
            return false;
        }

        options = new Options(pattern, paths, printMatchesOnly, recursiveSearch);
        return true;
    }
}