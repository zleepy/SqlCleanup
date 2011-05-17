using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlCleanup.SqlParser
{
    class Lexer
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
                new ExactMatchRule("+", TokenType.Comma),
                new ExactMatchRule("-", TokenType.Point),
                new ExactMatchRule("*", TokenType.StartParenthesis),
                new ExactMatchRule("/", TokenType.EndParenthesis),

                new NumberRule
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

        public void Tokenize()
        {

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
    };    
}
