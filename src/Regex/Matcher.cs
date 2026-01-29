namespace codecrafters_grep.Regex;

public sealed class Matcher
{
    public int Match(Node pattern, string input, int startPosition, out RuntimeState endState)
    {
        var state = new RuntimeState { Position = startPosition };
        bool matched = TryMatchNode(pattern, input, state, () => true);
        endState = state;
        return matched ? state.Position - startPosition : -1;
    }

    private bool TryMatchNode(Node node, string input, RuntimeState state, Func<bool> continuation)
    {
        return node switch
        {
            Sequence seq => TryMatchSequence(seq.Parts, 0, input, state, continuation),
            Alternation alt => TryMatchAlternation(alt, input, state, continuation),
            Group group => TryMatchGroup(group, input, state, continuation),
            Quantifier quant => TryMatchQuantifier(quant, input, state, continuation),
            Literal lit => TryMatchLiteral(lit, input, state, continuation),
            AnyChar any => TryMatchAny(any, input, state, continuation),
            CharClass cls => TryMatchCharClass(cls, input, state, continuation),
            AnchorStart anchor => TryMatchAnchorStart(anchor, input, state, continuation),
            AnchorEnd anchor => TryMatchAnchorEnd(anchor, input, state, continuation),
            Backref backref => TryMatchBackref(backref, input, state, continuation),
            _ => false
        };
    }

    private bool TryMatchSequence(IReadOnlyList<Node> parts, int index, string input, RuntimeState state, Func<bool> continuation)
    {
        if (index >= parts.Count)
            return continuation();

        var part = parts[index];
        var snapshot = state.Clone();
        if (TryMatchNode(part, input, state, () => TryMatchSequence(parts, index + 1, input, state, continuation)))
            return true;

        state.Restore(snapshot);
        return false;
    }

    private bool TryMatchAlternation(Alternation alt, string input, RuntimeState state, Func<bool> continuation)
    {
        foreach (var option in alt.Alternatives)
        {
            var snapshot = state.Clone();
            if (TryMatchNode(option, input, state, continuation))
                return true;

            state.Restore(snapshot);
        }

        return false;
    }

    private bool TryMatchGroup(Group group, string input, RuntimeState state, Func<bool> continuation)
    {
        var snapshot = state.Clone();
        int start = state.Position;
        if (TryMatchNode(group.Inner, input, state, () =>
            {
                state.Captures[group.Index] = input.Substring(start, state.Position - start);
                return continuation();
            }) == false)
        {
            state.Restore(snapshot);
            return false;
        }

        return true;
    }

    private bool TryMatchQuantifier(Quantifier quant, string input, RuntimeState state, Func<bool> continuation)
    {
        var snapshots = new List<RuntimeState> { state.Clone() };
        int count = 0;

        while (quant.Max == null || count < quant.Max.Value)
        {
            var before = state.Clone();
            int start = state.Position;
            if (TryMatchNode(quant.Inner, input, state, () => true) == false)
            {
                state.Restore(before);
                break;
            }

            if (state.Position == start)
                break;

            count++;
            snapshots.Add(state.Clone());
        }

        if (count < quant.Min)
            return false;

        for (int i = snapshots.Count - 1; i >= 0; i--)
        {
            if (i < quant.Min)
                break;

            state.Restore(snapshots[i]);
            if (continuation())
                return true;
        }

        return false;
    }

    private bool TryMatchLiteral(Literal lit, string input, RuntimeState state, Func<bool> continuation)
    {
        if (state.Position >= input.Length)
            return false;

        if (input[state.Position] != lit.C)
            return false;

        state.Position += 1;
        return continuation();
    }

    private bool TryMatchAny(AnyChar _, string input, RuntimeState state, Func<bool> continuation)
    {
        if (state.Position >= input.Length)
            return false;

        if (input[state.Position] == '\n')
            return false;

        state.Position += 1;
        return continuation();
    }

    private bool TryMatchCharClass(CharClass cls, string input, RuntimeState state, Func<bool> continuation)
    {
        if (state.Position >= input.Length)
            return false;

        bool match = cls.Chars.Contains(input[state.Position]);
        if (cls.Negated)
            match = !match;

        if (match == false)
            return false;

        state.Position += 1;
        return continuation();
    }

    private bool TryMatchAnchorStart(AnchorStart _, string input, RuntimeState state, Func<bool> continuation)
    {
        return state.Position == 0 && continuation();
    }

    private bool TryMatchAnchorEnd(AnchorEnd _, string input, RuntimeState state, Func<bool> continuation)
    {
        return state.Position == input.Length && continuation();
    }

    private bool TryMatchBackref(Backref backref, string input, RuntimeState state, Func<bool> continuation)
    {
        if (state.Captures.TryGetValue(backref.Index, out var captured) == false)
            return false;

        if (state.Position + captured.Length > input.Length)
            return false;

        if (input.AsSpan(state.Position, captured.Length).SequenceEqual(captured) == false)
            return false;

        state.Position += captured.Length;
        return continuation();
    }
}
