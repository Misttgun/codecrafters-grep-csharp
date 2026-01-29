namespace codecrafters_grep.Regex;

public sealed class RegexEngine
{
    private readonly Node _pattern;
    private readonly Matcher _matcher = new();

    public RegexEngine(string pattern)
    {
        var parser = new RegexParser();
        _pattern = parser.Parse(pattern);
    }

    public int MatchAt(string input, int startPosition)
    {
        int consumed = _matcher.Match(_pattern, input, startPosition, out _);
        return consumed == 0 ? 0 : consumed;
    }
}
