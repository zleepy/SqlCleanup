using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SqlCleanup.Parser
{
    [DebuggerDisplay("[{Start}-{End}:{Length} {Type} '{ToString()}']")]
    public class Token
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int Length
        {
            get { return End - Start; }
            set { End = Start + value; }
        }
        public SubArray<char> Content { get; set; }
        public TokenType Type{ get; set; }

        public Token()
        {
        }

        public Token(char[] source, int start, int end, TokenType type)
        {
            Start = start;
            End = end;
            Type = type;
            Content = new SubArray<char>(source, Start, Length);
        }

        public override string ToString()
        {
            return new string(Content.Copy());
        }
    }

    public enum TokenType
    {
        Unknown,
        SpecialCharacter,
        StartParenthesis,
        EndParenthesis,
        StartSquareBracket,
        EndSquareBracket,
        Word,
        Litteral,
        StringLitteral,
        Variable,
        Comma,
        Comment,
        SepparatorPoint,
    }

    //[DebuggerDisplay("Text = {Text}")]
    //public class Token
    //{
    //    public Token Parent { get; set; }
    //    public int StartPos { get; set; }
    //    public int EndPos { get; set; }
    //    public List<Token> Tokens { get; set; }
    //    public string Text { get; set; }

    //    public Token()
    //    {
    //        Tokens = new List<Token>();
    //    }

    //    public Token(Token parent)
    //        : this()
    //    {
    //        Parent = parent;
    //        Tokens = new List<Token>();
    //    }

    //    public Token CreateSibbling()
    //    {
    //        var newToken = new Token(Parent);

    //        if (Parent != null)
    //            Parent.Tokens.Add(newToken);

    //        return newToken;
    //    }
    //}
    
    //public class Block : Token
    //{
    //    public Block()
    //        : base()
    //    {
    //    }

    //    public Block(Token parent)
    //        : base(parent)
    //    {
    //    }
    //}

    //public class Word : Token
    //{
    //    public Word()
    //        : base()
    //    {
    //    }

    //    public Word(Token parent)
    //        : base(parent)
    //    {
    //    }
    //}

    //public class Litteral : Token
    //{
    //    public Litteral()
    //        : base()
    //    {
    //    }

    //    public Litteral(Token parent)
    //        : base(parent)
    //    {
    //    }
    //}
}
