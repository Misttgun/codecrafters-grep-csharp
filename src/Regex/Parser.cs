namespace codecrafters_grep.Regex;

public sealed class RegexParser
{
    public Node Parse(string pattern)
    {
        if (pattern == null)
            throw new ArgumentNullException(nameof(pattern));

        var reader = new PatternReader(pattern);
        var parser = new ParserImpl(reader);
        return parser.Parse();
    }

    private sealed class ParserImpl
    {
        private readonly PatternReader _reader;
        private int _nextCaptureIndex;

        public ParserImpl(PatternReader reader)
        {
            _reader = reader;
            _nextCaptureIndex = 0;
        }

        public Node Parse()
        {
            var node = ParseSequence();
            if (_reader.HasMore)
                throw new ArgumentException("Unexpected trailing input in pattern.");

            return node;
        }

        private Node ParseSequence()
        {
            var parts = new List<Node>();
            while (_reader.HasMore && _reader.Peek() != ')' && _reader.Peek() != '|')
            {
                var atom = ParseAtom();
                parts.Add(ParseQuantifierIfAny(atom));
            }

            if (_reader.HasMore && _reader.Peek() == '|')
            {
                var alternatives = new List<Node> { new Sequence(parts) };
                while (_reader.HasMore && _reader.Peek() == '|')
                {
                    _reader.Read();
                    alternatives.Add(ParseSequence());
                }

                return new Alternation(alternatives);
            }

            return new Sequence(parts);
        }

        private Node ParseAtom()
        {
            if (_reader.Peek() == '^')
            {
                _reader.Read();
                return new AnchorStart();
            }

            if (_reader.Peek() == '$')
            {
                _reader.Read();
                return new AnchorEnd();
            }

            if (_reader.Peek() == '.')
            {
                _reader.Read();
                return new AnyChar();
            }

            if (_reader.Peek() == '[')
                return ParseCharClass();

            if (_reader.Peek() == '(')
                return ParseGroup();

            if (_reader.Peek() == '\\')
                return ParseEscape();

            return new Literal(_reader.Read());
        }

        private Node ParseGroup()
        {
            _reader.Read();
            int index = ++_nextCaptureIndex;
            var inner = ParseSequence();

            if (_reader.HasMore == false || _reader.Peek() != ')')
                throw new ArgumentException("Unclosed group in pattern.");

            _reader.Read();
            return new Group(index, inner);
        }

        private Node ParseEscape()
        {
            _reader.Read();
            if (_reader.HasMore == false)
                throw new ArgumentException("Dangling escape in pattern.");

            char c = _reader.Read();
            if (char.IsDigit(c))
                return new Backref(c - '0');

            return c switch
            {
                'w' => new CharClass("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_", false),
                'd' => new CharClass("0123456789", false),
                _ => new Literal(c)
            };
        }

        private Node ParseCharClass()
        {
            _reader.Read();
            bool negated = _reader.HasMore && _reader.Peek() == '^';
            if (negated)
                _reader.Read();

            var start = _reader.Position;
            while (_reader.HasMore && _reader.Peek() != ']')
                _reader.Read();

            if (_reader.HasMore == false)
                throw new ArgumentException("Unclosed character class in pattern.");

            var chars = _reader.Slice(start, _reader.Position);
            _reader.Read();
            return new CharClass(chars, negated);
        }

        private Node ParseQuantifierIfAny(Node atom)
        {
            if (_reader.HasMore == false)
                return atom;

            return _reader.Peek() switch
            {
                '+' => ConsumeQuantifier(atom, 1, null),
                '*' => ConsumeQuantifier(atom, 0, null),
                '?' => ConsumeQuantifier(atom, 0, 1),
                '{' => ParseRange(atom),
                _ => atom
            };
        }

        private Node ConsumeQuantifier(Node atom, int min, int? max)
        {
            _reader.Read();
            return new Quantifier(atom, min, max);
        }

        private Node ParseRange(Node atom)
        {
            _reader.Read();
            int start = _reader.Position;
            while (_reader.HasMore && _reader.Peek() != '}')
                _reader.Read();

            if (_reader.HasMore == false)
                throw new ArgumentException("Unclosed range quantifier in pattern.");

            var body = _reader.Slice(start, _reader.Position);
            _reader.Read();

            var parts = body.Split(',');
            if (parts.Length == 1)
            {
                int n = int.Parse(parts[0]);
                return new Quantifier(atom, n, n);
            }

            int min = int.Parse(parts[0]);
            int? max = parts[1].Length == 0 ? null : int.Parse(parts[1]);
            return new Quantifier(atom, min, max);
        }
    }

    private sealed class PatternReader
    {
        private readonly string _pattern;

        public PatternReader(string pattern)
        {
            _pattern = pattern;
            Position = 0;
        }

        public int Position { get; private set; }
        public bool HasMore => Position < _pattern.Length;

        public char Peek()
        {
            return _pattern[Position];
        }

        public char Read()
        {
            return _pattern[Position++];
        }

        public string Slice(int start, int endExclusive)
        {
            return _pattern[start..endExclusive];
        }
    }
}
