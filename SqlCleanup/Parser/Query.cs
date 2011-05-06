using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SqlCleanup.Parser
{
    [DebuggerDisplay("Text = {Text}")]
    public class Token
    {
        public Token Parent { get; set; }
        public int StartPos { get; set; }
        public int EndPos { get; set; }
        public List<Token> Tokens { get; set; }
        public string Text { get; set; }

        public Token()
        {
            Tokens = new List<Token>();
        }

        public Token(Token parent)
            : this()
        {
            Parent = parent;
            Tokens = new List<Token>();
        }

        public Token CreateSibbling()
        {
            var newToken = new Token(Parent);

            if (Parent != null)
                Parent.Tokens.Add(newToken);

            return newToken;
        }
    }
    
    public class Block : Token
    {
        public Block()
            : base()
        {
        }

        public Block(Token parent)
            : base(parent)
        {
        }
    }

    public class Word : Token
    {
        public Word()
            : base()
        {
        }

        public Word(Token parent)
            : base(parent)
        {
        }
    }

    public class Litteral : Token
    {
        public Litteral()
            : base()
        {
        }

        public Litteral(Token parent)
            : base(parent)
        {
        }
    }
}
