using System;
using System.Collections.Generic;

namespace SqlCleanup.SqlParser
{
    class ExactMatchRule : Rule
    {
        public string MatchString { get; private set; }

        public ExactMatchRule(string match, TokenType tokenType)
            : base(tokenType)
        {
            MatchString = match;
        }

        public override bool Match(Lexer lexer, out State result)
        {
            int startPos = lexer.Pos;
            if (IsMatchingString(lexer, MatchString))
            {
                result = new State(Type, startPos, lexer.Pos - startPos);
                return true;
            }

            result = new State();
            return false;
        }
    }

    class ExactMatchManyRule : Rule
    {
        public string[] MatchString { get; private set; }

        public ExactMatchManyRule(string[] match, TokenType tokenType)
            : base(tokenType)
        {
            MatchString = match;
        }

        public override bool Match(Lexer lexer, out State result)
        {
            int startPos = lexer.Pos;
            foreach (var m in MatchString)
            {
                if (IsMatchingString(lexer, m))
                {
                    result = new State(Type, startPos, lexer.Pos - startPos);
                    return true;
                }
            }

            result = new State();
            return false;
        }
    }

    class WordRule : Rule
    {
        public IDictionary<string, TokenType> SpecialWords { get; private set; }

        public WordRule(TokenType tokenType, params Tuple<string[], TokenType>[] specialWords)
            : base(tokenType)
        {
            SpecialWords = new Dictionary<string, TokenType>();
            foreach (var specialGroup in specialWords)
            {
                foreach (var word in specialGroup.Item1)
                    SpecialWords.Add(word.ToUpper(), specialGroup.Item2);
            }
        }

        public override bool Match(Lexer lexer, out State result)
        {
            if (!lexer.IsLetter)
            {
                result = new State();
                return false;
            }

            int startPos = lexer.Pos;

            while (lexer.Next())
            {
                if (!(lexer.IsLetterOrDigit || lexer.Current == '_'))
                    break;
            }

            var foundWord = lexer.GetSubArray(startPos, lexer.Pos - startPos).ToString().ToUpper();
            result = new State(SpecialWords.ContainsKey(foundWord) ? SpecialWords[foundWord] : Type, startPos, lexer.Pos - startPos);
            return true;
        }
    }
}
