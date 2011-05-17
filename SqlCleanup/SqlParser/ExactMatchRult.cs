
namespace SqlCleanup.SqlParser
{
    class ExactMatchRule : Rule
    {
        public string Match { get; private set; }

        public ExactMatchRule(string match, TokenType tokenType)
            : base(tokenType)
        {
            Match = match;
        }

        public override bool Match(Lexer lexer, out State result)
        {
            int startPos = lexer.Pos;
            if (IsMatchingString(lexer, Match))
            {
                result = new State(startPos, lexer.Pos - startPos);
                return true;
            }

            result = new State();
            return false;
        }
    }
}
