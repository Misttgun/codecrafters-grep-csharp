if (args[0] != "-E")
{
    Console.WriteLine("Expected first argument to be '-E'");
    Environment.Exit(2);
}

// Temporary: Simulate piped input for debugging
if (Console.IsInputRedirected == false)
{
    Console.SetIn(new StringReader("I see 2 cot"));
}

string pattern = args[1];
string inputLine = Console.In.ReadToEnd();

if (MatchPattern(inputLine, pattern))
    Environment.Exit(0);
else
    Environment.Exit(1);

return;

static int MatchHere(string pattern, string inputLine)
{
    if (pattern.Length == 0)
        return 0;

    // Handle end of string anchor
    if (pattern == "$")
        return inputLine.Length == 0 ? 0 : -1;

    // Identify the "Atom" (the unit to match)
    Func<string, int> atomMatcher;
    int atomPatternLength;

    if (pattern.StartsWith('('))
    {
        var endIndex = pattern.IndexOf(')', 1);
        if (endIndex == -1) 
            return -1;

        var subPatterns = pattern[1..endIndex].Split('|');
        atomPatternLength = endIndex + 1;
        atomMatcher = input => MatchAlternationGroup(subPatterns, input);
    }
    else if (pattern.StartsWith('['))
    {
        var endIndex = pattern.IndexOf(']', 1);
        if (endIndex == -1) 
            return -1;

        var isNegated = pattern[1] == '^';
        var startIndex = isNegated ? 2 : 1;
        var chars = pattern[startIndex..endIndex];
        atomPatternLength = endIndex + 1;

        atomMatcher = input =>
        {
            if (input.Length == 0) 
                return -1;
            
            bool match = chars.Contains(input[0]);
            if (isNegated)
                match = !match;
            
            return match ? 1 : -1;
        };
    }
    else if (pattern.StartsWith("\\w"))
    {
        atomPatternLength = 2;
        atomMatcher = input => (input.Length > 0 && IsWordChar(input[0])) ? 1 : -1;
    }
    else if (pattern.StartsWith("\\d"))
    {
        atomPatternLength = 2;
        atomMatcher = input => (input.Length > 0 && char.IsDigit(input[0])) ? 1 : -1;
    }
    else if (pattern[0] == '.')
    {
        atomPatternLength = 1;
        atomMatcher = input => (input.Length > 0 && input[0] != '\n') ? 1 : -1;
    }
    else // Literal
    {
        if (IsLiteralChar(pattern[0]) == false) 
            return -1;
        
        char expected = pattern[0];
        atomPatternLength = 1;
        atomMatcher = input => (input.Length > 0 && input[0] == expected) ? 1 : -1;
    }

    // Check for Quantifiers and Execute
    var remainingPattern = pattern[atomPatternLength..];

    if (pattern.Length > atomPatternLength)
    {
        if (pattern[atomPatternLength] == '+')
            return MatchOneOrMore(atomMatcher, remainingPattern[1..], inputLine);

        if (pattern[atomPatternLength] == '?')
            return MatchZeroOrOne(atomMatcher, remainingPattern[1..], inputLine);
    }

    // Default: Match Exactly Once
    int consumed = atomMatcher(inputLine);
    if (consumed == -1) 
        return -1;

    int restConsumed = MatchHere(remainingPattern, inputLine[consumed..]);
    return restConsumed != -1 ? consumed + restConsumed : -1;
}

static int MatchOneOrMore(Func<string, int> atomMatcher, string remainingPattern, string inputLine)
{
    int totalAtomConsumed = 0;
    var consumptionHistory = new List<int>(); // Track how much each step consumed for backtracking

    // Greedy match: consume as many atoms as possible
    while (true)
    {
        int consumed = atomMatcher(inputLine[totalAtomConsumed..]);
        if (consumed == -1) 
            break;

        totalAtomConsumed += consumed;
        consumptionHistory.Add(consumed);
    }

    if (consumptionHistory.Count == 0) 
        return -1;

    // Backtrack
    while (consumptionHistory.Count > 0)
    {
        int restConsumed = MatchHere(remainingPattern, inputLine[totalAtomConsumed..]);
        if (restConsumed != -1)
            return totalAtomConsumed + restConsumed;

        // Backtrack: give up the last matched atom
        int lastStep = consumptionHistory[^1];
        consumptionHistory.RemoveAt(consumptionHistory.Count - 1);
        totalAtomConsumed -= lastStep;
    }

    return -1;
}

static int MatchZeroOrOne(Func<string, int> atomMatcher, string remainingPattern, string inputLine)
{
    // Try matching one
    int consumed = atomMatcher(inputLine);
    if (consumed != -1)
    {
        int restConsumed = MatchHere(remainingPattern, inputLine[consumed..]);
        if (restConsumed != -1)
            return consumed + restConsumed;
    }

    // Try matching zero
    return MatchHere(remainingPattern, inputLine);
}

static int MatchAlternationGroup(string[] subPatterns, string inputLine)
{
    foreach (var subPattern in subPatterns)
    {
        int consumed = MatchHere(subPattern, inputLine);

        // If the sub-pattern matched, it must return how much it consumed.
        // Note: MatchHere typically processes the *rest* of the pattern too.
        // But here, 'subPattern' IS the whole pattern for the group.
        // So if it returns success, the return value is exactly the length consumed by the group.
        if (consumed != -1)
            return consumed;
    }

    return -1;
}

static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

static bool IsLiteralChar(char c) => Globals.BannedChars.Contains(c) == false;

static bool MatchPattern(string inputLine, string pattern)
{
    var shouldMatchStart = pattern.StartsWith('^');
    if (shouldMatchStart)
    {
        pattern = pattern[1..];
        int matchCount = MatchHere(pattern, inputLine);
        if (matchCount != -1)
        {
            Console.WriteLine(inputLine[..matchCount]);
            return true;
        }
    }

    for (var i = 0; i < inputLine.Length; i++)
    {
        int matchCount = MatchHere(pattern, inputLine[i..]);
        if (matchCount != -1)
        {
            Console.WriteLine(inputLine[i..(i + matchCount)]);
            return true;
        }
    }

    return false;
}

internal static class Globals
{
    public static readonly List<char> BannedChars = ['+', '.', '?', '|', '^', '$', '[', ']', '(', ')', '{', '}', '\\'];
}