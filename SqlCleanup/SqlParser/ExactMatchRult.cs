
namespace SqlCleanup.SqlParser
{
    class ExactMatchRule : Rule
    {
        public string MatchString { get; private set; }

        public ExactMatchRule(string match, TokenType tokenType)
            : base(tokenType)
        {
            MatchString = match;
        }

        public override bool Match(Lexer lexer, out State result)
        {
            int startPos = lexer.Pos;
            if (IsMatchingString(lexer, MatchString))
            {
                result = new State(startPos, lexer.Pos - startPos);
                return true;
            }

            result = new State();
            return false;
        }
    }

    class ExactMatchManyRule : Rule
    {
        public string[] MatchString { get; private set; }

        public ExactMatchManyRule(string[] match, TokenType tokenType)
            : base(tokenType)
        {
            MatchString = match;
        }

        public override bool Match(Lexer lexer, out State result)
        {
            int startPos = lexer.Pos;
            foreach (var m in MatchString)
            {
                if (IsMatchingString(lexer, m))
                {
                    result = new State(startPos, lexer.Pos - startPos);
                    return true;
                }
            }

            result = new State();
            return false;
        }
    }
}
