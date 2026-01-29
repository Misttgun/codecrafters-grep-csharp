namespace codecrafters_grep.Regex;

public abstract record Node;

public sealed record Sequence(IReadOnlyList<Node> Parts) : Node;

public sealed record Alternation(IReadOnlyList<Node> Alternatives) : Node;

public sealed record Group(int Index, Node Inner) : Node;

public sealed record Literal(char C) : Node;

public sealed record AnyChar() : Node;

public sealed record CharClass(string Chars, bool Negated) : Node;

public sealed record AnchorStart() : Node;

public sealed record AnchorEnd() : Node;

public sealed record Backref(int Index) : Node;

public sealed record Quantifier(Node Inner, int Min, int? Max) : Node;
