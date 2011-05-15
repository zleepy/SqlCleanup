using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlCleanup.Parser
{
    public class SqlFormatter
    {
        public NewLineLocation NewLineAtSelectArgumentSeparator { get; set; }
        public NewLineLocation NewLineAtFrom { get; set; }
        public NewLineLocation NewLineAtWhere { get; set; }
        public NewLineLocation NewLineAtOrderBy { get; set; }
        public NewLineLocation NewLineAtHaving { get; set; }
        public NewLineLocation NewLineAtJoin { get; set; }

        public IndentationType IndentationType { get; set; }
        public string Indentation { get; set; }
        public string NewLine { get; set; }
        public int LineLengthHint { get; set; }

        public SqlFormatter()
        {
            NewLineAtSelectArgumentSeparator = NewLineLocation.After;
            NewLineAtFrom = NewLineLocation.Before;
            NewLineAtWhere = NewLineLocation.Before;
            NewLineAtOrderBy = NewLineLocation.Before;
            NewLineAtHaving = NewLineLocation.Before;
            NewLineAtJoin = NewLineLocation.Before;

            LineLengthHint = 80;
            Indentation = "    ";
            NewLine = Environment.NewLine;
        }

        public string Format(IEnumerable<Token> sqlTokens)
        {
            var result = new StringBuilder();
            var tokens = sqlTokens.ToArray();

            for (int i = 0; i < tokens.Length; i++)
			{
                var t = tokens[i];

                if (t.Type == TokenType.Word)
                {
                    if (t == "SELECT")
                        FormatSelect(tokens, ref i, result);
                    else if (t == "FROM")
                        FormatFrom(tokens, ref i, result);
                    else if (t == "WHERE")
                        FormatWhere(tokens, ref i, result);
                    else if (t == "HAVING")
                        FormatHaving(tokens, ref i, result);
                    else if (t == "ORDER" && tokens[i + 1] == "BY")
                        FormatOrderBy(tokens, ref i, result);
                }

            }

            return result.ToString();
        }

        private void FormatSelect(Token[] tokens, ref int i, StringBuilder result)
        {
            result.Append("SELECT");

            for (; i < tokens.Length; i++)
            {
                var t = tokens[i];
                if (t.Type == TokenType.EndOfQuery || (t.Type == TokenType.Word && t == "FROM"))
                {
                    i--;
                    break;
                }

                if (t.Type == TokenType.Comma)
                {
                    AppendWithNewLines(result, NewLineAtSelectArgumentSeparator, t.Content.ToString());
                }
                else
                {
                    result.Append(" ");
                    result.Append(t.Content);
                }
            }
        }

        private void FormatFrom(Token[] tokens, ref int i, StringBuilder result)
        {
            AppendWithNewLines(result, NewLineAtFrom, "FROM");

            for (; i < tokens.Length; i++)
            {
                var t = tokens[i];
                if (t.Type == TokenType.EndOfQuery || 
                    (t.Type == TokenType.Word && (t == "WHERE" ||
                    t == "HAVING") ||
                    (t == "ORDER" && tokens[i + 1] == "BY") ||
                    (t == "GROUP" && tokens[i + 1] == "BY")))
                {
                    i--;
                    break;
                }

                if (t.Type == TokenType.Comma)
                {
                    AppendWithNewLines(result, NewLineAtSelectArgumentSeparator, t.Content.ToString());
                }
                else
                {
                    result.Append(" ");
                    result.Append(t.Content);
                }
            }
        }

        private static void AppendWithNewLines(StringBuilder result, NewLineLocation newLineLocation, string content)
        {
            if (newLineLocation.HasFlag(NewLineLocation.Before))
                result.AppendLine();

            result.Append(content);

            if (newLineLocation.HasFlag(NewLineLocation.After))
                result.AppendLine();
        }

        private void FormatWhere(Token[] tokens, ref int i, StringBuilder result)
        {
            throw new NotImplementedException();
        }

        private void FormatHaving(Token[] tokens, ref int i, StringBuilder result)
        {
            throw new NotImplementedException();
        }

        private void FormatOrderBy(Token[] tokens, ref int i, StringBuilder result)
        {
            throw new NotImplementedException();
        }

        private void FormatJoin(Token[] tokens, ref int i, StringBuilder result)
        {
            throw new NotImplementedException();
        }

        private bool IsJoin(Token[] tokens, int i)
        {
            var t1 = tokens[i];

            if (t1 == "JOIN")
                return true;

            if (tokens.Length > i + 1)
            {
                if ((t1 == "OUTER" || t1 == "INNER") && tokens[i + 1] == "JOIN")
                    return true;
            }

            if (tokens.Length <= i + 2)
                return false;

            var t2 = tokens[i + 1];
            var t3 = tokens[i + 2];

            return (t1 == "LEFT" && (t2 == "OUTER" || t2 == "INNER") && t3 == "JOIN") || (t1 == "RIGHT" && (t2 == "OUTER" || t2 == "INNER") && t3 == "JOIN");
        }

        private bool IsBlockStartWord(Token t)
        {
            if (t.Type != TokenType.Word)
                return false;

            switch (t.ToString().ToUpper())
            {
                case "SELECT":
                case "FROM":
                case "WHERE":
                case "HAVING":
                    return true;
            }
            return false;
        }
    }

    public enum NewLineLocation
    {
        None = 0,
        Before = 1,
        After = 2,
        BeforeAndAfter = Before | After,
        BeforeIfToLong = 9,
    }

    public enum IndentationType
    {
        AlignWithBlockParent,
        Indentation,
    }
}
