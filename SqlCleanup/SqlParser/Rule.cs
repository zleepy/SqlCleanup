using System;

namespace SqlCleanup.SqlParser
{
    abstract class Rule
    {
        public TokenType Type { get; private set; }

        public abstract bool Match(Lexer lexer, out State result);

        public Rule(TokenType tokenType)
        {
            Type = tokenType;
        }

        protected bool IsMatchingString(Lexer lexer, string text)
        {
            if (lexer.Current == text[0])
            {
                if (text.Length == 1)
                {
                    lexer.Next();
                    return true;
                }

                if (lexer.Peek == text[1])
                {
                    if (text.Length == 2)
                    {
                        lexer.Next();
                        lexer.Next();
                        return true;
                    }
                    else
                    {
                        throw new NotImplementedException("Har inte implementerat lämförelse för strängar än två tecken.");
                    }
                }
            }
            return false;
        }
    }
}
