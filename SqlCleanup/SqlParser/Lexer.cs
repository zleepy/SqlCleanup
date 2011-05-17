using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlCleanup.SqlParser
{
    class Lexer //: IEnumerable<Token>
    {
        public List<Rule> Rules { get; private set; }
        protected readonly char[] text;
        private readonly int length = 0;
        private char current = '\0';
        private char peek = '\0';

        public int Pos { get; protected set; }
        public int Length { get { return length; } }
        public char Current { get { return current; } }
        public bool IsNummeric { get { return char.IsNumber(Current); } }
        public bool IsLetter { get { return char.IsLetter(Current); } }
        public bool IsLetterOrDigit { get { return char.IsLetterOrDigit(Current); } }
        public bool IsWhiteSpace { get { return char.IsWhiteSpace(Current); } }
        public char Peek { get { return peek; } }

        public Lexer(string text)
            : this()
        {
            this.text = text.ToCharArray();
            this.length = this.text.Length;

            if (length > 0)
                peek = text[0];

            Pos = -1;
        }

        public Lexer()
        {
            Rules = new List<Rule>()
            {
                new MatchingStartEndRule("/*", "*/", TokenType.Comment),
                new MatchingStartEndRule("--", "\r", TokenType.Comment),
                new MatchingStartEndRule("'", "'", '\\', TokenType.LitteralText),
                new MatchingStartEndRule("[", "]", TokenType.Identifier),
                new MatchingStartEndRule("\"", "\"", TokenType.Identifier),
                new ExactMatchRule(",", TokenType.Comma),
                new ExactMatchRule(".", TokenType.Point),
                new ExactMatchRule("(", TokenType.StartParenthesis),
                new ExactMatchRule(")", TokenType.EndParenthesis),
                new ExactMatchRule("+", TokenType.Plus),
                new ExactMatchRule("-", TokenType.Minus),
                new ExactMatchRule("*", TokenType.Star),
                new ExactMatchRule("/", TokenType.ForwardSlash),
                new ExactMatchRule("=", TokenType.Equal),
                new ExactMatchRule("<>", TokenType.NotEqual),
                new ExactMatchRule("<", TokenType.LessThan),
                new ExactMatchRule(">", TokenType.GreaterThan),
                new NumberRule(TokenType.Number),
            };
        }

        public bool Next()
        {
            current = peek;

            if (current == '\0')
                return false;

            Pos++;

            if (Pos + 1 >= length)
            {
                peek = '\0';
            }
            else
            {
                peek = text[Pos + 1];

            //    for (int i = Pos + 1; i < length; i++)
            //    {
            //        var c = text[i];
            //        if (!char.IsWhiteSpace(c))
            //        {
            //            peekNextNonWhiteSpace = c;
            //        }
            //    }
            }

            return true;
        }

        public List<Token> Tokenize()
        {
            var result = new List<Token>();

            Next();

            while (Current != '\0')
            {
                bool foundMatch = false;
                State state;

                foreach (var rule in Rules)
                {
                    if (rule.Match(this, out state))
                    {
                        result.Add(new Token(state.Start, state.Count, text, rule.Type, state.HasError));

                        foundMatch = true;
                        break;
                    }
                }

                if (!foundMatch)
                {
                    result.Add(new Token(Pos, 1, text, TokenType.Unknown));
                    Next();
                }
            }

            return result;
        }
    }

    public enum TokenType
    {
        Unknown,
        Comment,
        LitteralText,
        Identifier,
        Comma,
        Point,
        StartParenthesis,
        EndParenthesis,
        Number,
        Plus,
        Minus,
        Star,
        ForwardSlash,
        Equal,
        NotEqual,
        LessThan,
        GreaterThan,
    };    
}
