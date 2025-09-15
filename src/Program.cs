using System;
using System.IO;

if (args[0] != "-E")
{
    Console.WriteLine("Expected first argument to be '-E'");
    Environment.Exit(2);
}

string pattern = args[1];
string inputLine = Console.In.ReadToEnd();

if (MatchPattern(inputLine, pattern))
    Environment.Exit(0);
else
    Environment.Exit(1);

return;

static bool MatchPattern(string inputLine, string pattern)
{
    if (pattern == "\\d")
        return inputLine.Any(char.IsDigit);
    
    if (pattern.Length == 1)
        return inputLine.Contains(pattern);

    throw new ArgumentException($"Unhandled pattern: {pattern}");
}
