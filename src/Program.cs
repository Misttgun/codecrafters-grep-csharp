using System.Text;
using codecrafters_grep.Regex;


if (TryParseArgs(args, out var options) == false)
    Environment.Exit(2);

var matchedAnyLine = false;

while (Console.In.ReadLine() is { } line)
{
    var result = MatchPattern(line, options.Pattern, options.PrintMatchesOnly);
    if (string.IsNullOrEmpty(result) == false)
    {
        matchedAnyLine = true;
        Console.Write(result);
    }
}

if (options.Paths.Count > 0)
{
    var paths = new List<string>();
    if (options.RecursiveSearch)
    {
        if (Directory.Exists(options.Paths[0]))
            paths.AddRange(Directory.EnumerateFiles(options.Paths[0], "*.txt", SearchOption.AllDirectories));
    }
    else
    {
        paths = options.Paths;
    }
    
    bool multiplePaths = paths.Count > 1;
    foreach (var path in paths)
    {
        if (Path.Exists(path) == false)
            continue;
        
        foreach (var line in File.ReadAllLines(path))
        {
            var result = MatchPattern(line, options.Pattern, options.PrintMatchesOnly);
            if (string.IsNullOrEmpty(result) == false)
            {
                matchedAnyLine = true;
                Console.Write(multiplePaths ? $"{path}:{result}" : result);
            }
        }
    }
}

Environment.Exit(matchedAnyLine ? 0 : 1);
return;

static string MatchPattern(string inputLine, string pattern, bool printMatched)
{
    StringBuilder builder = new StringBuilder();

    foreach (var (start, length) in FindMatches(inputLine, pattern))
    {
        if (printMatched)
        {
            builder.AppendLine(inputLine.Substring(start, length));
        }
        else
        {
            builder.AppendLine(inputLine);
            break;
        }
    }

    return builder.ToString();
}

static IEnumerable<(int Start, int Length)> FindMatches(string inputLine, string pattern)
{
    var engine = new RegexEngine(pattern);
    bool anchoredToStart = pattern.StartsWith('^');

    if (anchoredToStart)
    {
        int len = engine.MatchAt(inputLine, 0);
        if (len != -1)
            yield return (0, len);
        yield break;
    }

    var i = 0;
    while (i <= inputLine.Length)
    {
        int len = engine.MatchAt(inputLine, i);
        if (len == -1)
        {
            i++;
            continue;
        }

        yield return (i, len);
        i += len > 0 ? len : 1;
    }
}
    
static bool TryParseArgs(string[] args, out Options options)
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
    List<string> paths = [];
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

internal readonly record struct Options(string Pattern, List<string> Paths, bool PrintMatchesOnly, bool RecursiveSearch);