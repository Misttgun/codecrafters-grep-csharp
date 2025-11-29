if (args[0] != "-E")
{
    Console.WriteLine("Expected first argument to be '-E'");
    Environment.Exit(2);
}

// Temporary: Simulate piped input for debugging
if (Console.IsInputRedirected == false)
{
    Console.SetIn(new StringReader("I see 1 cat"));
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
    if (pattern.Length == 0)
        return true;

    // Handle end of string anchor
    if (pattern == "$")
        return inputLine.Length == 0;

    if (pattern.StartsWith('('))
    {
        var index = pattern.IndexOf(')', 1);
        if (index == -1)
            return false;

        var subPatterns = pattern[1..index].Split('|');
        return MatchAlternation(pattern[(index + 1)..], inputLine, subPatterns);
    }

    if (pattern.StartsWith("\\w"))
        return MatchToken(pattern, inputLine, 2, IsWordChar);

    if (pattern.StartsWith("\\d"))
        return MatchToken(pattern, inputLine, 2, char.IsDigit);

    if (pattern.StartsWith('['))
    {
        var index = pattern.IndexOf(']', 1);
        if (index == -1)
            return false;

        var isNegated = pattern[1] == '^';
        var startIdx = isNegated ? 2 : 1;
        var chars = pattern[startIdx..index];

        return MatchToken(pattern, inputLine, index + 1, ch =>
        {
            bool match = chars.Any(v => ch.Equals(v));
            return isNegated ? !match : match;
        });
    }

    var matchChar = pattern[0];

    // Handle wildcard
    if (matchChar == '.')
        return MatchToken(pattern, inputLine, 1, c => c != '\n');

    // Handle literal character
    if (IsLiteralChar(matchChar))
        return MatchToken(pattern, inputLine, 1, c => c == matchChar);

    return false;
}

static bool MatchToken(string pattern, string inputLine, int tokenLength, Func<char, bool> matcher)
{
    // Check for quantifiers first
    if (pattern.Length > tokenLength)
    {
        var nextChar = pattern[tokenLength];
        if (nextChar == '+')
            return MatchOneOrMore(pattern[(tokenLength + 1)..], inputLine, matcher);
        if (nextChar == '?')
            return MatchZeroOrOne(pattern[(tokenLength + 1)..], inputLine, matcher);
    }

    // No quantifier: try to match exactly one character
    if (inputLine.Length > 0 && matcher(inputLine[0]))
        return MatchHere(pattern[tokenLength..], inputLine[1..]);

    return false;
}

static bool MatchOneOrMore(string remainingPattern, string inputLine, Func<char, bool> matcher)
{
    // Match as many characters as possible (greedy)
    int matchCount = 0;

    for (int i = 0; i < inputLine.Length && matcher(inputLine[i]); i++)
        matchCount++;

    // Try to match the rest of the pattern, backtracking from the longest match
    for (int i = matchCount; i >= 1; i--)
    {
        if (MatchHere(remainingPattern, inputLine[i..]))
            return true;
    }

    return false;
}

static bool MatchZeroOrOne(string remainingPattern, string inputLine, Func<char, bool> matcher)
{
    int matchCount = 0;

    for (int i = 0; i < inputLine.Length && matcher(inputLine[i]); i++)
        matchCount++;

    if (matchCount > 1)
        return false;

    // Try to match the rest of the pattern
    for (int i = matchCount; i >= 1; i--)
    {
        if (MatchHere(remainingPattern, inputLine[i..]))
            return true;
    }

    return MatchHere(remainingPattern, inputLine);
}

static bool MatchAlternation(string remainingPattern, string inputLine, string[] subPatterns)
{
    foreach (var subPattern in subPatterns)
    {
        if (MatchHere(subPattern, inputLine))
        {
            int matchedLength = subPattern.Length;
            return MatchHere(remainingPattern, inputLine[matchedLength..]);
        }
    }

    return false;
}

static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

static bool IsLiteralChar(char c) => Globals.BannedChars.Contains(c) == false;

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

internal static class Globals
{
    public static readonly List<char> BannedChars = ['+', '.', '?', '|', '^', '$', '[', ']', '(', ')', '{', '}', '\\'];
}