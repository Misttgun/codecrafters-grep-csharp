using System.Text;

namespace codecrafters_grep;

public static class Matcher
{
    private static readonly List<char> BannedChars = ['+', '.', '?', '|', '^', '$', '[', ']', '(', ')', '{', '}', '\\'];

    public static string MatchPattern(string inputLine, string pattern, bool printMatched)
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

    private static IEnumerable<(int Start, int Length)> FindMatches(string inputLine, string pattern)
    {
        var anchoredToStart = pattern.StartsWith('^');
        if (anchoredToStart)
            pattern = pattern[1..];

        if (anchoredToStart)
        {
            var len = MatchHere(pattern, inputLine);
            if (len != -1)
                yield return (0, len);

            yield break;
        }

        var i = 0;
        while (i <= inputLine.Length)
        {
            var len = MatchHere(pattern, inputLine[i..]);
            if (len == -1)
            {
                i++;
                continue;
            }

            yield return (i, len);

            i += len > 0 ? len : 1; // progress guarantee for "$" and other empty matches
        }
    }

    private static int MatchHere(string pattern, string inputLine)
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
            
            if (pattern[atomPatternLength] == '*')
                return MatchZeroOrMore(atomMatcher, remainingPattern[1..], inputLine);
        }

        // Default: Match Exactly Once
        int consumed = atomMatcher(inputLine);
        if (consumed == -1)
            return -1;

        int restConsumed = MatchHere(remainingPattern, inputLine[consumed..]);
        return restConsumed != -1 ? consumed + restConsumed : -1;
    }

    private static int MatchOneOrMore(Func<string, int> atomMatcher, string remainingPattern, string inputLine)
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

    private static int MatchZeroOrOne(Func<string, int> atomMatcher, string remainingPattern, string inputLine)
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

    private static int MatchZeroOrMore(Func<string, int> atomMatcher, string remainingPattern, string inputLine)
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

        // Try matching zero
        if (consumptionHistory.Count == 0)
            return MatchHere(remainingPattern, inputLine);

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

    private static int MatchAlternationGroup(string[] subPatterns, string inputLine)
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

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    private static bool IsLiteralChar(char c) => BannedChars.Contains(c) == false;
}