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
    if (pattern.Length == 0)
        return true;

    if (inputLine.Length == 0)
        return pattern == "$";

    var c = inputLine[0];

    if (pattern.StartsWith("\\w"))
    {
        if (IsWordChar(c) == false)
            return false;

        // Process quantifier after the character class
        if (pattern.Length > 2)
        {
            switch (pattern[2])
            {
                case '+':
                    return MatchOneOrMore(pattern[3..], inputLine, IsWordChar);
                case '?':
                    return MatchZeroOrOne(pattern[3..], inputLine, IsWordChar);
            }
        }

        return MatchHere(pattern[2..], inputLine[1..]);
    }

    if (pattern.StartsWith("\\d"))
    {
        if (char.IsDigit(c) == false)
            return false;

        // Process quantifier after the character class
        if (pattern.Length > 2)
        {
            switch (pattern[2])
            {
                case '+':
                    return MatchOneOrMore(pattern[3..], inputLine, char.IsDigit);
                case '?':
                    return MatchZeroOrOne(pattern[3..], inputLine, char.IsDigit);
            }
        }

        return MatchHere(pattern[2..], inputLine[1..]);
    }
    
    if (pattern.StartsWith('['))
    {
        var index = pattern.IndexOf(']', 1);
        if (index == -1)
            return false;

        var isNegated = pattern[1] == '^';
        var startIdx = isNegated ? 2 : 1;
        var chars = pattern[startIdx..index];

        var match = chars.Any(v => c.Equals(v));
        if (isNegated)
            match = !match;

        if (match == false)
            return false;

        // Check if there's a + quantifier after the character class
        if (pattern.Length > index + 1)
        {
            switch (pattern[index + 1])
            {
                case '+':
                    return MatchOneOrMore(pattern[(index + 2)..], inputLine, IsCharInWord);
                case '?':
                    return MatchZeroOrOne(pattern[(index + 2)..], inputLine, IsCharInWord);
            }
        }

        return MatchHere(pattern[(index + 1)..], inputLine[1..]);

        // Local function to check if a character is in a word
        bool IsCharInWord(char ch)
        {
            bool match = chars.Any(v => ch.Equals(v));
            return isNegated ? !match : match;
        }
    }

    // Handle literal character
    var matchChar = pattern[0];
    if (IsLiteralChar(matchChar) == false)
        return false;
    
    if (pattern.Length > 1)
    {
        switch (pattern[1])
        {
            case '+':
                return MatchOneOrMore(pattern[2..], inputLine, c => c == matchChar);
            case '?':
                return MatchZeroOrOne(pattern[2..], inputLine, c => c == matchChar);
        }
    }
    
    return matchChar == c && MatchHere(pattern[1..], inputLine[1..]);
}

static bool MatchOneOrMore(string remainingPattern, string inputLine, Func<char, bool> matcher)
{
    // Match as many characters as possible (greedy)
    int matchCount = 0;

    for (int i = 0; i < inputLine.Length && matcher(inputLine[i]); i++)
        matchCount++;

    // Try to match the rest of the pattern, backtracking from longest match
    for (int i = matchCount; i >= 1; i--)
    {
        if (MatchHere(remainingPattern, inputLine[i..]))
            return true;
    }

    return false;
}

static bool MatchZeroOrOne(string remainingPattern, string inputLine, Func<char, bool> matcher)
{
    // Match as many characters as possible (greedy)
    int matchCount = 0;

    for (int i = 0; i < inputLine.Length && matcher(inputLine[i]); i++)
        matchCount++;
    
    if (matchCount > 1)
        return false;

    // Try to match the rest of the pattern, backtracking from longest match
    for (int i = matchCount; i >= 1; i--)
    {
        if (MatchHere(remainingPattern, inputLine[i..]))
            return true;
    }

    return MatchHere(remainingPattern, inputLine);
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