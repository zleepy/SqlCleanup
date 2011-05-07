using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlCleanup.Parser
{
    public class SqlLexer
    {
        public static char[] WhiteSpace = { ' ', '\t', '\n', '\r' };
        public static char[] SpecialCharacters = { '*', '?', '+', '-', '*', '/', '%' };
        public static char StartParenthesis = '(';
        public static char EndParenthesis = ')';
        public static char StartSquareBracket = '[';
        public static char EndSquareBracket = ']';
        public static char StringLitteralStart = '\'';
        public static char StringLitteralEnd = '\'';
        public static char StringLitteralEscape = '\\';
        public static char[] CommentRestOfLine = { '-', '-' };
        public static char[] CommentStart = { '/', '*' };
        public static char[] CommentEnd = { '*', '/' };
        public static char DecimalPoint = '.';
        public static char SepparatorPoint = '.';
        public static char Comma = ',';
        public static char StartVariable = '@';
        public static char[] EndOfLine = { '\r', '\n' };

        private class LexerIndex
        {
            public char[] Text { get; set; }
            public int Index { get; private set; }
            public char Current { get; private set; }
            public char? Peek { get; private set; }
            public char? PeekPeek { get; private set; }

            public LexerIndex(char[] text)
            {
                Index = -1;
                Text = text;
            }

            public bool Next()
            {
                if (Index >= Text.Length)
                {
                    Current = char.MinValue;
                    return false;
                }

                Index++;

                if (Index >= Text.Length)
                {
                    Current = char.MinValue;
                    return false;
                }

                if (!Peek.HasValue)
                {
                    Current = Text[Index];

                    if (Index + 1 >= Text.Length)
                        Peek = null;
                    else
                        Peek = Text[Index + 1];
                }
                else
                {
                    Current = Peek.Value;
                    Peek = PeekPeek;
                }

                if (Index + 2 >= Text.Length)
                    PeekPeek = null;
                else
                    PeekPeek = Text[Index + 2];

                return true;
            }
        }

        public Token[] TokenizeFile(string path)
        {
            return Tokenize(System.IO.File.ReadAllText(path));
        }

        public Token[] Tokenize(string text)
        {
            return Tokenize(text.ToCharArray());
        }

        public Token[] Tokenize(char[] text)
        {
            var index = new LexerIndex(text);
            var tokens = new List<Token>();

            while (index.Next())
            {
                if (!MoveToFirstNonWhite(index))
                    break;

                else if (IsSpecialCharacter(index.Current))
                {
                    if (index.Current == CommentRestOfLine[0] && index.Current == CommentRestOfLine[1])
                        tokens.Add(ReadCommentRestOfLine(index));
                    else if (index.Current == CommentStart[0] && index.Current == CommentStart[1])
                        tokens.Add(ReadComment(index));
                    else
                        tokens.Add(CreateSingelCharacterToken(index, TokenType.SpecialCharacter));
                }
                else if (IsDigit(index.Current))
                {
                    tokens.Add(ReadNumber(index));
                }
                else if (index.Current == StartVariable)
                {
                    tokens.Add(ReadVariable(index));
                }
                else if (IsLetter(index.Current))
                {
                    tokens.Add(ReadWord(index));
                }
                else if (index.Current == StartParenthesis)
                {
                    tokens.Add(CreateSingelCharacterToken(index, TokenType.StartParenthesis));
                }
                else if (index.Current == EndParenthesis)
                {
                    tokens.Add(CreateSingelCharacterToken(index, TokenType.EndParenthesis));
                }
                else if (index.Current == StartSquareBracket)
                {
                    tokens.Add(CreateSingelCharacterToken(index, TokenType.StartSquareBracket));
                }
                else if (index.Current == EndSquareBracket)
                {
                    tokens.Add(CreateSingelCharacterToken(index, TokenType.EndSquareBracket));
                }
                else if (index.Current == SepparatorPoint)
                {
                    tokens.Add(CreateSingelCharacterToken(index, TokenType.SepparatorPoint));
                }
                else if (index.Current == Comma)
                {
                    tokens.Add(CreateSingelCharacterToken(index, TokenType.Comma));
                }
                else if (index.Current == StringLitteralStart)
                {
                    tokens.Add(ReadStringLitteral(index));
                }
                else
                {
                    tokens.Add(CreateSingelCharacterToken(index, TokenType.Unknown));
                }
            }

            return tokens.ToArray();
        }

        private Token ReadComment(LexerIndex index)
        {
            var start = index.Index;

            index.Next();

            while (index.Next())
            {
                if (index.Current == CommentEnd[0])
                {
                    if (index.Peek == CommentEnd[1])
                    {
                        index.Next();
                        break;
                    }
                }
            }

            return new Token(index.Text, start, index.Index + 1, TokenType.Comment);
        }

        private Token ReadCommentRestOfLine(LexerIndex index)
        {
            var start = index.Index;

            while (index.Next())
            {
                if (index.Current == EndOfLine[0])
                {
                    if (index.Peek == EndOfLine[1])
                        index.Next();
                    break;
                }
            }

            return new Token(index.Text, start, index.Index + 1, TokenType.Comment);
        }

        private Token ReadStringLitteral(LexerIndex index)
        {
            var start = index.Index;
            bool lastWasEscape = false;

            while (index.Next())
            {
                if (lastWasEscape)
                    lastWasEscape = false;
                else if (index.Current == StringLitteralEscape)
                    lastWasEscape = true;
                else if (index.Current == StringLitteralEnd)
                    return new Token(index.Text, start, index.Index - 1, TokenType.StringLitteral);
            }

            return new Token(index.Text, start, index.Index - 1, TokenType.Unknown);
        }

        private Token ReadWord(LexerIndex index)
        {
            var start = index.Index;

            while (IsLetterOrDigit(index.Peek))
            {
                index.Next();
            }

            return new Token(index.Text, start, index.Index + 1, TokenType.Word);
        }

        private Token ReadVariable(LexerIndex index)
        {
            if (!index.PeekPeek.HasValue)
                return CreateSingelCharacterToken(index, TokenType.Unknown);

            var start = index.Index;

            while (IsLetterOrDigit(index.Peek))
            {
                index.Next();
            }

            return new Token(index.Text, start, index.Index + 1, TokenType.Variable);
        }

        private Token ReadNumber(LexerIndex index)
        {
            var start = index.Index;

            while (IsDigit(index.Peek) || index.Current == DecimalPoint)
            {
                index.Next();
            }

            return new Token(index.Text, start, index.Index + 1, TokenType.Litteral);
        }

        private static Token CreateSingelCharacterToken(LexerIndex index, TokenType type)
        {
            return new Token(index.Text, index.Index, index.Index + 1, type);
        }

        private bool MoveToFirstNonWhite(LexerIndex index)
        {
            while (IsWhiteSpace(index.Current))
            {
                if (!index.Next())
                    return false;
            }
            return true;
        }

        private bool IsWhiteSpace(char? c)
        {
            return c.HasValue && WhiteSpace.Contains(c.Value);
        }

        private bool IsSpecialCharacter(char? c)
        {
            return c.HasValue && SpecialCharacters.Contains(c.Value);
        }

        private bool IsDigit(char? c)
        {
            return c.HasValue && char.IsDigit(c.Value);
        }

        private bool IsLetter(char? c)
        {
            return c.HasValue && char.IsLetter(c.Value);
        }

        private bool IsLetterOrDigit(char? c)
        {
            return c.HasValue && (IsLetter(c) || IsDigit(c));
        }
    }
}
