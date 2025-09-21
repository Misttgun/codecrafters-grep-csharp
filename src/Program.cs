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
    if(pattern == "\\w")
        return inputLine.Any( c => char.IsLetterOrDigit(c) || c == '_');
    
    if (pattern == "\\d")
        return inputLine.Any(char.IsDigit);
    
    if (pattern.Length == 1)
        return inputLine.Contains(pattern);

    if (pattern.StartsWith('[') && pattern.EndsWith(']'))
    {
        var chars = pattern.Substring(1, pattern.Length - 2);
        Console.WriteLine(chars);
        return chars.Any(c => inputLine.Contains(c));
    }

    throw new ArgumentException($"Unhandled pattern: {pattern}");
}
