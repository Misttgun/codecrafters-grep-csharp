namespace codecrafters_grep;

public static class Parser
{
    public readonly record struct Options(string Pattern, bool PrintMatchesOnly);
    
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

        // First non-flag token is the pattern.
        var pattern = args.FirstOrDefault(a => a.Length > 0 && a[0] != '-');
        if (string.IsNullOrEmpty(pattern))
        {
            Console.WriteLine("Expected a pattern argument");
            return false;
        }

        options = new Options(pattern, printMatchesOnly);
        return true;
    }
}