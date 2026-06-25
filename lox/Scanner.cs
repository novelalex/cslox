namespace cslox.lox {
    class Scanner(string source)
    {
        private readonly string source = source;
        private readonly List<Token> tokens = [];
        private int start = 0;
        private int current = 0;
        private int line = 1;

        private static readonly Dictionary<string, TokenType> keywords;

        static Scanner(){
            keywords = new Dictionary<string, TokenType>
            {
                ["and"] = TokenType.AND,
                ["class"] = TokenType.CLASS,
                ["else"] = TokenType.ELSE,
                ["false"] = TokenType.FALSE,
                ["for"] = TokenType.FOR,
                ["fun"] = TokenType.FUN,
                ["if"] = TokenType.IF,
                ["nil"] = TokenType.NIL,
                ["or"] = TokenType.OR,
                ["print"] = TokenType.PRINT,
                ["return"] = TokenType.RETURN,
                ["super"] = TokenType.SUPER,
                ["this"] = TokenType.THIS,
                ["true"] = TokenType.TRUE,
                ["var"] = TokenType.VAR,
                ["while"] = TokenType.WHILE
            };
        }

        public List<Token> ScanTokens() {
            while (!IsAtEnd()) {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        private bool IsAtEnd() {
            return current >= source.Length;
        }

        private void ScanToken() {
            char c = Advance();
            switch (c) {
                // Single-character tokens
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;

                // One or two character tokens
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '/':
                    if (Match('/')) {
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    } else if (Match('*')) {
                        HandleBlockComment();
                    } else {
                        AddToken(TokenType.SLASH);
                    }
                    break;

                // Whitespace
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    line++;
                    break;

                // Strings
                case '"': HandleString(); break;

                default:
                    if (IsDigit(c)) {
                        HandleNumber();
                    } else if (IsAlpha(c)){
                        HandleIdentifier();
                    } else {
                        Lox.Error(line, "Unexpected character");
                    }
                    break;
            }
        }

        private char Advance() {
            // Post-increment operator matters here
            return source[current++];
        }

        private void AddToken(TokenType type) {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, Object? literal) {
            string text = source[start..current];
            tokens.Add(new Token(type, text, literal, line));
        }

        private bool Match(char expected) {
            if (IsAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        private char Peek() {
            if (IsAtEnd()) return '\0';
            return source[current];
        }

        private void HandleString() {
            while (Peek() != '"' && !IsAtEnd()) {
                if (Peek() == '\n') line++;
                Advance();
            }

            if (IsAtEnd()) {
                Lox.Error(line, "Unterminated string.");
                return;
            }

            // Consume closing '"'
            Advance();

            // Trim surrounding quotes
            string value = source[(start+1)..(current-1)];

            // TODO(novel): Handle escape codes

            AddToken(TokenType.STRING, value);
        }

        private static bool IsDigit(char c) {
            return c >= '0' && c <= '9';
        }

        private void HandleNumber() {
            while (IsDigit(Peek())) Advance();

            if (Peek() == '.' && IsDigit(PeekNext())) {
                // Consume the '.'
                Advance();

                while (IsDigit(Peek())) Advance();
            }

            AddToken(TokenType.NUMBER, double.Parse(source[start..current]));
        }

        private char PeekNext() {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        private void HandleIdentifier() {
            while (IsAlphaNumeric(Peek())) Advance();
            
            string text = source[start..current];
            TokenType? type = keywords.TryGetValue(text, out var t) ? t : null; 
            AddToken(type.GetValueOrDefault(TokenType.IDENTIFIER));
        }

        private static bool IsAlpha(char c) {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   (c == '_'); 
        }

        private static bool IsAlphaNumeric(char c) {
            return IsAlpha(c) || IsDigit(c);
        }

        private void HandleBlockComment() {
            int depth = 0;

            while (!IsAtEnd()) {
                if ((Peek() == '*') && (PeekNext() == '/')) {
                    if (depth == 0) {
                        break;
                    }
                    Advance();
                    depth--;
                } else if ((Peek() == '/') && (PeekNext() == '*')) {
                    Advance();
                    depth++;
                } else if (Peek() == '\n') {
                    line++;
                }
                Advance();
            }

            if (IsAtEnd()) {
                Lox.Error(line, "Unterminated block comment.");
                return;
            }

            // Consume closing '*/'
            Advance();
            Advance();

        }
    }
}