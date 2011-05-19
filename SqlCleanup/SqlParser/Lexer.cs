using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlCleanup.Parser;

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
                new ExactMatchRule("/", TokenType.Division),
                new ExactMatchRule("%", TokenType.Percent),
                new ExactMatchRule("=", TokenType.Equal),
                new ExactMatchRule("<>", TokenType.NotEqual),
                new ExactMatchRule("!=", TokenType.NotEqual),
                new ExactMatchRule("<=", TokenType.LessThanOrEqual),
                new ExactMatchRule(">=", TokenType.GreaterThanOrEqual),
                new ExactMatchRule("<", TokenType.LessThan),
                new ExactMatchRule(">", TokenType.GreaterThan),
                new ExactMatchRule("!<", TokenType.NotLessThan),
                new ExactMatchRule("!>", TokenType.NotGreaterThan),
                //new ExactMatchRule("&", TokenType.BitwiseOperator),
                //new ExactMatchRule("|", TokenType.BitwiseOperator),
                //new ExactMatchRule("^", TokenType.BitwiseOperator),
                new ExactMatchManyRule(new[] {"&", "^", "|"}, TokenType.BitwiseOperator),
                new ExactMatchRule("+=", TokenType.PlusEquals),
                new ExactMatchRule("-=", TokenType.MinusEquals),
                new ExactMatchRule("*=", TokenType.StarEquals),
                new ExactMatchRule("/=", TokenType.DivisionEquals),
                new ExactMatchRule("%=", TokenType.PercentEquals),
                new ExactMatchManyRule(new[] {"&=", "^=", "|="}, TokenType.BitwiseOperatorEquals),
                //new ExactMatchRule(, TokenType.BitwiseOperatorEquals),
                //new ExactMatchRule(, TokenType.BitwiseOperatorEquals),
                new NumberRule(TokenType.Number),
                new WordRule(TokenType.Word, 
                    new Tuple<string[], TokenType>(new[] { "ALL", "AND", "ANY", "BETWEEN", "EXISTS", "IN", "LIKE", "NOT", "OR", "SOME" }, TokenType.LogicOperators),
                    new Tuple<string[], TokenType>(new[] { "ABS", "DATEDIFF", "POWER", "ACOS", "DAY", "RADIANS", "ASIN", "DEGREES", "ROUND", "ATAN", "EXP", "SIGN", "ATN2", "FLOOR", "SIN", "CEILING", "ISNULL", "SQUARE", "COALESCE", "ISNUMERIC", "SQRT", "COS", "LOG", "TAN", "COT", "LOG10", "YEAR", "DATALENGTH", "MONTH", "DATEADD", "NULLIF" }, TokenType.BuiltInFunction),
                    new Tuple<string[], TokenType>(new[] { "AVG", "MIN", "CHECKSUM_AGG", "OVER", "COUNT", "ROWCOUNT_BIG", "COUNT_BIG", "STDEV", "GROUPING", "STDEVP", "GROUPING_ID", "SUM", "MAX", "VAR", "VARP" }, TokenType.AggregateFunction),
                    new Tuple<string[], TokenType>(new[] { "ASCII", "NCHAR", "SOUNDEX", "CHAR", "PATINDEX", "SPACE", "CHARINDEX", "QUOTENAME", "STR", "DIFFERENCE", "REPLACE", "STUFF", "LEFT", "REPLICATE", "SUBSTRING", "LEN", "REVERSE", "UNICODE", "LOWER", "RIGHT", "UPPER", "LTRIM", "RTRIM" }, TokenType.StringFunction)),

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

        public SubArray<char> GetSubArray(int offset, int count)
        {
            return new SubArray<char>(text, offset, count);
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
        Division,
        Equal,
        NotEqual,
        LessThan,
        GreaterThan,
        BitwiseOperator,
        Percent,
        LessThanOrEqual,
        GreaterThanOrEqual,
        NotGreaterThan,
        NotLessThan,
        BitwiseOperatorEquals,
        PlusEquals,
        MinusEquals,
        StarEquals,
        DivisionEquals,
        PercentEquals,
        Word,
        LogicOperators,
        BuiltInFunction,
        AggregateFunction,
        StringFunction,
    };    
}
