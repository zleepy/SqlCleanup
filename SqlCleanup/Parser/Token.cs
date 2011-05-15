using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SqlCleanup.Parser
{
    [DebuggerDisplay("[{Start}-{End}:{Length} {Type} '{ToString()}']")]
    public struct Token
    {
        public int Start;//{ get; set; }
        public int End;//{ get; set; }
        public int Length
        {
            get { return End - Start; }
            set { End = Start + value; }
        }
        public SubArray<char> Content;// { get; set; }
        public TokenType Type;//{ get; set; }

        //public Token()
        //{
        //}

        public Token(char[] source, int start, int end, TokenType type)
        {
            Start = start;
            End = end;
            Type = type;
            Content = new SubArray<char>(source, Start, End - Start /*Length*/);
        }

        public override string ToString()
        {
            return new string(Content.Copy());
        }

        public static bool operator==(Token x, string y)
        {
            return x.ToString() == y;
        }

        public static bool operator!=(Token x, string y)
        {
            return x.ToString() != y;
        }

        public override bool Equals(object obj)
        {
            if (obj is string)
                return ToString().Equals((string)obj, StringComparison.InvariantCultureIgnoreCase);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ToString().ToUpper().GetHashCode();
        }
    }

    public enum TokenType
    {
        Unknown,
        SpecialCharacter,
        StartParenthesis,
        EndParenthesis,
        StartIdentifierDelimiter,
        EndIdentifierDelimiter,
        Word,
        Litteral,
        StringLitteral,
        Variable,
        Comma,
        Comment,
        SepparatorPoint,
        EndOfQuery,
    }
}
