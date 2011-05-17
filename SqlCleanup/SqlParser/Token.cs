using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlCleanup.SqlParser
{
    struct Token
    {
        public readonly TokenType Type;
        public readonly bool HasError;
        public readonly SqlCleanup.Parser.SubArray<char> Selection;

        public Token(int offset, int count, char[] source, TokenType type, bool hasError = false)
        {
            Type = type;
            HasError = hasError;
            Selection = new Parser.SubArray<char>(source, offset, count);
        }
    }
}
