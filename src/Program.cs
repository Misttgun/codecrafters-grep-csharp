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

static bool MatchHere(string pattern, string inputLine)
{
    while (true)
    {
        if (pattern.Length == 0)
            return true;
        
        if (pattern[0] == '$')
            return inputLine.Length == 0;

        if (inputLine.Length == 0)
            return false;

        var c = inputLine[0];

        if (pattern.StartsWith("\\w") && (char.IsLetterOrDigit(c) || c == '_'))
        {
            pattern = pattern[2..];
            inputLine = inputLine[1..];
            continue;
        }

        if (pattern.StartsWith("\\d") && char.IsDigit(c))
        {
            pattern = pattern[2..];
            inputLine = inputLine[1..];
            continue;
        }

        if (pattern.StartsWith("[^"))
        {
            var index = pattern.IndexOf(']', 2);
            if (index == -1)
                return false;

            var chars = pattern[2..index];
            if (chars.Any(value => c.Equals(value)))
                return false;

            pattern = pattern[(index + 1)..];
            inputLine = inputLine[1..];
            continue;
        }

        if (pattern.StartsWith('['))
        {
            var index = pattern.IndexOf(']', 1);
            if (index == -1)
                return false;

            var chars = pattern[2..index];
            var match = chars.Any(v => c.Equals(v));
            if (match == false)
                return false;

            pattern = pattern[(index + 1)..];
            inputLine = inputLine[1..];
            continue;
        }

        if (pattern[0] == c)
        {
            pattern = pattern[1..];
            inputLine = inputLine[1..];
            continue;
        }

        return false;
    }
}

static bool MatchPattern(string inputLine, string pattern)
{
    var shouldMatchStart = pattern.StartsWith('^');
    if (shouldMatchStart)
    {
        pattern = pattern[1..];
        return MatchHere(pattern, inputLine);       
    }
    
    for (var i = 0; i < inputLine.Length; i++)
    {
        if (MatchHere(pattern, inputLine[i..]))
            return true;
    }

    return false;
}