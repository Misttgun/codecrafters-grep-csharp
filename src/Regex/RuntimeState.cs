namespace codecrafters_grep.Regex;

public sealed class RuntimeState
{
    public int Position { get; set; }
    public Dictionary<int, string> Captures { get; } = new();

    public RuntimeState Clone()
    {
        var clone = new RuntimeState { Position = Position };
        foreach (var kvp in Captures)
            clone.Captures[kvp.Key] = kvp.Value;
        return clone;
    }

    public void Restore(RuntimeState snapshot)
    {
        Position = snapshot.Position;
        Captures.Clear();
        foreach (var kvp in snapshot.Captures)
            Captures[kvp.Key] = kvp.Value;
    }
}
