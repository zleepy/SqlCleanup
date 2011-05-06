using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlCleanup.Parser
{
    public class SqlTokenizer
    {
        private readonly Func<string, Lexer> lexterFactory;

        public SqlTokenizer(Func<string, Lexer> lexterCreator)
        {
            this.lexterFactory = lexterCreator;
        }

        public Token[] ProcessFile(string path)
        {
            return Process(System.IO.File.ReadAllText(path));
        }

        public Token[] Process(string text)
        {
            var lexer = lexterFactory(text);
            var resultQueries = new List<Token>();

            while (lexer.Next())
            {
                SkipWhiteSpace(lexer);

                if (lexer.IsLetterOrDigit || lexer.Current == '(')
                {
                    resultQueries.Add(ReadBlock(lexer));
                }
            }

            return resultQueries.ToArray();
        }

        private Token ReadBlock(Lexer lexer, Token parent = null)
        {
            var block = new Block();
            block.StartPos = lexer.Pos;
            bool startWithParenthesis = lexer.Current == '(';

            if(startWithParenthesis)
            {
                block.Tokens.Add(new Token(block) { Text = "(", StartPos = lexer.Pos, EndPos = lexer.Pos });
                if(!lexer.Next())
                {
                    block.EndPos = lexer.Pos;
                    return block;
                }
            }

            while(true)
            {
                if (lexer.IsLetter)
                {
                    block.Tokens.Add(ReadWord(lexer, block));
                }
                else if (lexer.IsNummeric)
                {
                    block.Tokens.Add(ReadLitteral(lexer, block));
                }
                else if (lexer.Current == '\'')
                {
                    block.Tokens.Add(ReadStringLitteral(lexer, block));
                }
                else if (lexer.Current == ')')
                {
                    block.Tokens.Add(new Token(block) { Text = ")", StartPos = lexer.Pos, EndPos = lexer.Pos });
                    block.EndPos = lexer.Pos;
                    return block;
                }
                else if(lexer.Current == '(')
                {
                    block.Tokens.Add(ReadBlock(lexer, block));
                }
                else if (lexer.IsWhiteSpace)
                {
                    // Ignorera mellanslag.
                }
                else
                {
                    block.Tokens.Add(new Token(block) { Text = lexer.Current.ToString(), StartPos = lexer.Pos, EndPos = lexer.Pos });
                }

                if (!lexer.Next())
                {
                    block.EndPos = lexer.Pos;
                    return block;
                }
            }
        }

        private Token ReadStringLitteral(Lexer lexer, Token parent)
        {
            if (lexer.Current != '\'')
                throw new ArgumentException("ReadStringLitteral: Lexer måste peka på en \"'\".");

            var litteral = new Litteral(parent);
            litteral.Text = "'";

            while (lexer.Next())
            {
                if (lexer.Current == '\'')
                {
                    litteral.Text += "'";
                    break;
                }

                litteral.Text += lexer.Current.ToString();
            }

            return litteral;
        }

        private Token ReadLitteral(Lexer lexer, Token parent)
        {
            if (!lexer.IsNummeric)
                throw new ArgumentException("ReadLitteral: Lexer måste peka på en siffra.");

            var litteral = new Litteral(parent);
            litteral.Text = lexer.Current.ToString();

            if (lexer.CanPeek && (char.IsNumber(lexer.Peek) || lexer.Peek == '.'))
            {
                while (lexer.Next())
                {
                    litteral.Text += lexer.Current.ToString();
                    if (!lexer.CanPeek || !(char.IsNumber(lexer.Peek) || lexer.Peek == '.'))
                    {
                        break;
                    }
                }
            }

            return litteral;
        }

        private Token ReadWord(Lexer lexer, Token parent)
        {
            if (!lexer.IsLetterOrDigit)
                throw new ArgumentException("ReadWord: Lexer måste peka på en bokstav eller siffra.");

            var litteral = new Litteral(parent);
            litteral.Text = lexer.Current.ToString();

            if (lexer.CanPeek && (char.IsLetterOrDigit(lexer.Peek) || lexer.Peek == '@' || lexer.Peek == '?'))
            {
                while (lexer.Next())
                {
                    litteral.Text += lexer.Current.ToString();
                    if (!lexer.CanPeek || !(char.IsLetterOrDigit(lexer.Peek) || lexer.Peek == '@' || lexer.Peek == '?'))
                    {
                        break;
                    }
                }
            }

            return litteral;
        }

        private bool SkipWhiteSpace(Lexer lexer)
        {
            while (lexer.IsWhiteSpace)
            {
                if (!lexer.Next())
                    return false;
            }
            return true;
        }
    }
}
