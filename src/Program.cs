using codecrafters_grep;


if (Parser.TryParseArgs(args, out var options) == false)
    Environment.Exit(2);

var matchedAnyLine = false;

while (Console.In.ReadLine() is { } line)
{
    var result = Matcher.MatchPattern(line, options.Pattern, options.PrintMatchesOnly);
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
            var result = Matcher.MatchPattern(line, options.Pattern, options.PrintMatchesOnly);
            if (string.IsNullOrEmpty(result) == false)
            {
                matchedAnyLine = true;
                Console.Write(multiplePaths ? $"{path}:{result}" : result);
            }
        }
    }
}

Environment.Exit(matchedAnyLine ? 0 : 1);