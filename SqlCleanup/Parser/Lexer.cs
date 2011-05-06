using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlCleanup.Parser
{
    public class Lexer
    {
        protected readonly char[] text;
        private readonly int length = 0;
        private char current = char.MinValue;
        private char? peek = null;
        private char? peekNextNonWhiteSpace = null;

        public int Pos { get; protected set; }
        public int Length { get { return length; } }
        public char Current { get { return current; } }
        public bool IsNummeric { get { return char.IsNumber(Current); } }
        public bool IsLetter { get { return char.IsLetter(Current); } }
        public bool IsLetterOrDigit { get { return char.IsLetterOrDigit(Current); } }
        public bool IsWhiteSpace { get { return char.IsWhiteSpace(Current); } }
        public bool CanPeek { get { return peek.HasValue; } }
        public char Peek { get { return peek.Value; } }
        public bool CanPeekNonCharacter { get { return peekNextNonWhiteSpace.HasValue; } }
        public char? PeekNonCharacter { get { return peekNextNonWhiteSpace.Value; } }

        public Lexer(string text)
        {
            this.text = text.ToCharArray();
            this.length = this.text.Length;
            Pos = -1;
        }

        public bool Next()
        {
            if (Pos + 1 >= length)
            {
                current = char.MinValue;
                return false;
            }

            Pos++;

            if(peek.HasValue)
                current = peek.Value;
            else
                current = text[Pos];

            peekNextNonWhiteSpace = null;
            if (Pos + 1 >= length)
            {
                peek = null;
            }
            else
            {
                peek = text[Pos + 1];

                for (int i = Pos + 1; i < length; i++)
                {
                    var c = text[i];
                    if(!char.IsWhiteSpace(c))
                    {
                        peekNextNonWhiteSpace = c;
                    }
                }
            }

            return true;
        }
    }
}
