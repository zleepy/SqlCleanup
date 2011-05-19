
namespace SqlCleanup.SqlParser
{
    class NumberRule : Rule
    {
        public NumberRule(TokenType tokenType)
            : base(tokenType)
        {
        }

        public override bool Match(Lexer lexer, out State result)
        {
            if (!lexer.IsNummeric)
            {
                result = new State();
                return false;
            }

            int startPos = lexer.Pos;

            while (lexer.Next())
            {
                if (!(lexer.IsNummeric || lexer.Current == '.'))
                    break;
            }

            result = new State(Type, startPos, lexer.Pos - startPos);
            return true;
        }
    }
}
