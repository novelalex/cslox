namespace cslox.lox {
    class Token(TokenType type, string? lexeme, object? literal, int line)
    {
        public readonly TokenType type = type;
        public readonly string? lexeme = lexeme;
        public readonly object? literal = literal;
        public readonly int line = line;

        public override string? ToString() {
            return $"{type} {lexeme} {literal}";
        }
    }
}