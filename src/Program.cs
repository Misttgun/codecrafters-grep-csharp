using codecrafters_grep;


if (Parser.TryParseArgs(args, out var options) == false)
    Environment.Exit(2);

var matchedAnyLine = false;
while (Console.In.ReadLine() is { } line)
{
    if (Matcher.MatchPattern(line, options.Pattern, options.PrintMatchesOnly))
        matchedAnyLine = true;
}

Environment.Exit(matchedAnyLine ? 0 : 1);