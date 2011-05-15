using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SqlCleanup.Parser
{
    public class SqlParser
    {
        public static string[] ReservedWords = {
            "ADD", "ALL", "ALTER", "AND", "ANY", "AS", "ASC", "AUTHORIZATION", "BACKUP", "BEGIN", "BETWEEN", "BREAK", 
            "BROWSE", "BULK", "BY", "CASCADE", "CASE", "CHECK", "CHECKPOINT", "CLOSE", "CLUSTERED", "COALESCE", "COLLATE", 
            "COLUMN", "COMMIT", "COMPUTE", "CONSTRAINT", "CONTAINS", "CONTAINSTABLE", "CONTINUE", "CONVERT", "CREATE", 
            "CROSS", "CURRENT", "CURRENT_DATE", "CURRENT_TIME", "CURRENT_TIMESTAMP", "CURRENT_USER", "CURSOR", "DATABASE", 
            "DBCC", "DEALLOCATE", "DECLARE", "DEFAULT", "DELETE", "DENY", "DESC", "DISK", "DISTINCT", "DISTRIBUTED", 
            "DOUBLE", "DROP", "DUMMY", "DUMP", "ELSE", "END", "ERRLVL", "ESCAPE", "EXCEPT", "EXEC", "EXECUTE", "EXISTS", 
            "EXIT", "FETCH", "FILE", "FILLFACTOR", "FOR", "FOREIGN", "FREETEXT", "FREETEXTTABLE", "FROM", "FULL", 
            "FUNCTION", "GOTO", "GRANT", "GROUP", "HAVING", "HOLDLOCK", "IDENTITY", "IDENTITY_INSERT", "IDENTITYCOL", 
            "IF", "IN", "INDEX", "INNER", "INSERT", "INTERSECT", "INTO", "IS", "JOIN", "KEY", "KILL", "LEFT", "LIKE", 
            "LINENO", "LOAD", "NATIONAL ", "NOCHECK", "NONCLUSTERED", "NOT", "NULL", "NULLIF", "OF", "OFF", "OFFSETS", 
            "ON", "OPEN", "OPENDATASOURCE", "OPENQUERY", "OPENROWSET", "OPENXML", "OPTION", "OR", "ORDER", "OUTER", 
            "OVER", "PERCENT", "PLAN", "PRECISION", "PRIMARY", "PRINT", "PROC", "PROCEDURE", "PUBLIC", "RAISERROR", 
            "READ", "READTEXT", "RECONFIGURE", "REFERENCES", "REPLICATION", "RESTORE", "RESTRICT", "RETURN", "REVOKE",
            "RIGHT", "ROLLBACK", "ROWCOUNT", "ROWGUIDCOL", "RULE", "SAVE", "SCHEMA", "SELECT", "SESSION_USER", "SET",
            "SETUSER", "SHUTDOWN", "SOME", "STATISTICS", "SYSTEM_USER", "TABLE", "TEXTSIZE", "THEN", "TO", "TOP", "TRAN",
            "TRANSACTION", "TRIGGER", "TRUNCATE", "TSEQUAL", "UNION", "UNIQUE", "UPDATE", "UPDATETEXT", "USE", "USER", 
            "VALUES", "VARYING", "VIEW", "WAITFOR", "WHEN", "WHERE", "WHILE", "WITH", "WRITETEXT"};

        public static string[] QueryStart = { "SELECT", "UPDATE", "INSERT", "DELETE", "DECLARE", "SET", "BEGIN" };

        public static string[][] SectionStart = { new[] { "SELECT" }, 
                                                  new[] { "UPDATE" }, 
                                                  new[] { "INSERT" }, 
                                                  new[] { "DELETE" }, 
                                                  new[] { "SET" }, 
                                                  new[] { "FROM" }, 
                                                  new[] { "WHERE" }, 
                                                  new[] { "HAVING" }, 
                                                  new[] { "ORDER", "BY" }, 
                                                  new[] { "GROUP", "BY" },
                                                  new[] { "VALUES" },
                                                  new[] { "(" },
                                              };

        public static string[][] JoinGroups = { new[] { "JOIN" },
                                                     new [] { "INNER", "JOIN" },
                                                     new [] { "LEFT", "INNER", "JOIN" },
                                                     new [] { "RIGHT", "INNER", "JOIN" },
                                                     new [] { "OUTER", "JOIN" },
                                                     new [] { "LEFT", "OUTER", "JOIN" },
                                                     new [] { "RIGHT", "OUTER", "JOIN" }
                                                 };

        public static string[] BuiltInFunctions = { };

        public SqlLexer Lexer { get; set; }

        public SqlParser(SqlLexer lexer)
        {
            Lexer = lexer;
        }

        public SqlSection[] Parse(string text)
        {
            var tokens = Lexer.Tokenize(text);
            var result = new List<SqlSection>();
            var parent = new SqlQuery(null);
            result.Add(parent);

            for (int i = 0; i < tokens.Length; i++)
            {
                if (IsMatch(tokens, i, "SELECT"))
                {
                    AddSelect(tokens, ref i, parent);
                }
                else if (IsMatch(tokens, i, "UPDATE"))
                {
                    AddUpdate(tokens, ref i, result, parent);
                }
                else if (tokens[i].Type == TokenType.EndOfQuery)
                {
                    parent = new SqlQuery(null);
                    result.Add(parent);
                    AddTokenSection(parent, tokens[i]);
                }
                else
                {
                    AddTokenSection(parent, tokens[i]);
                }
            }

            return result.ToArray();
        }

        private SqlSelect AddSelect(Token[] tokens, ref int i, SqlQuery parent)
        {
            var section = new SqlSelect(parent);
            section.Sections.Add(new SqlSection(section, new SqlToken(true, tokens[i])));
            parent.Sections.Add(section);

            for (i++; i < tokens.Length; i++)
            {
                if (IsMatch(tokens, i, "FROM"))
                {
                    AddFrom(tokens, ref i, parent);
                    break;
                }
                else if (tokens[i].Type == TokenType.StartParenthesis)
                {
                    if (IsMatch(tokens, i + 1, "SELECT"))
                    {
                        var query = new SqlQuery(section);
                        AddTokenSection(query, tokens[i]);
                        i++;
                        AddSelect(tokens, ref i, query);
                        section.Sections.Add(query);
                    }
                    else
                    {
                        AddParenthesisGroup(tokens, ref i, section);
                        continue;
                    }
                }
                else if (tokens[i].Type == TokenType.EndOfQuery || tokens[i].Type == TokenType.EndParenthesis)
                {
                    AddTokenSection(parent, tokens[i]);
                    break;
                }
                else if (IsQueryStart(tokens[i]))
                {
                    i--;
                    break;
                }

                AddTokenSection(section, tokens[i]);
            }
            return section;
        }

        private SqlFrom AddFrom(Token[] tokens, ref int i, SqlQuery parent)
        {
            var section = new SqlFrom(parent);
            section.Sections.Add(new SqlSection(section, new SqlToken(true, tokens[i])));
            parent.Sections.Add(section);

            for (i++; i < tokens.Length; i++)
            {
                if (IsMatch(tokens, i, "WHERE"))
                {
                    AddWhere(tokens, ref i, parent);
                    break;
                }
                else if (IsMatch(tokens, i, "ORDER", "BY"))
                {
                    AddOrderBy(tokens, ref i, parent);
                    break;
                }
                else if (IsMatch(tokens, i, "GROUP", "BY"))
                {
                    AddGroupBy(tokens, ref i, parent);
                    break;
                }
                //else if (IsMatch(tokens, i, "JOIN") ||
                //    (IsMatch(tokens, i, "LEFT") && tokens[i + 1].Type != TokenType.StartParenthesis) ||
                //    (IsMatch(tokens, i, "RIGHT") && tokens[i + 1].Type != TokenType.StartParenthesis) ||
                //    IsMatch(tokens, i, "FULL") || 
                //    IsMatch(tokens, i, "INNER") || 
                //    IsMatch(tokens, i, "OUTER"))
                //{
                //    AddJoin(tokens, ref i, result, parent);
                //    break;
                //}
                else if (tokens[i].Type == TokenType.EndOfQuery || tokens[i].Type == TokenType.EndParenthesis)
                {
                    AddTokenSection(parent, tokens[i]);
                    break;
                }
                else if (IsQueryStart(tokens[i]))
                {
                    i--;
                    break;
                }

                AddTokenSection(section, tokens[i]);
            }
            return section;
        }

        private SqlGroupBy AddGroupBy(Token[] tokens, ref int i, SqlQuery parent)
        {
            var section = new SqlGroupBy(parent);
            section.Sections.Add(new SqlSection(section, new SqlToken(true, tokens[i])));
            parent.Sections.Add(section);

            for (i++; i < tokens.Length; i++)
            {
                if (IsMatch(tokens, i, "ORDER", "BY"))
                {
                    AddOrderBy(tokens, ref i, parent);
                    break;
                }
                else if (tokens[i].Type == TokenType.EndOfQuery || tokens[i].Type == TokenType.EndParenthesis)
                {
                    AddTokenSection(parent, tokens[i]);
                    break;
                }
                else if (IsQueryStart(tokens[i]))
                {
                    i--;
                    break;
                }

                AddTokenSection(section, tokens[i]);
            }
            return section;
        }

        private SqlOrderBy AddOrderBy(Token[] tokens, ref int i, SqlQuery parent)
        {
            var section = new SqlOrderBy(parent);
            section.Sections.Add(new SqlSection(section, new SqlToken(true, tokens[i])));
            parent.Sections.Add(section);

            for (i++; i < tokens.Length; i++)
            {
                if (IsMatch(tokens, i, "GROUP", "BY"))
                {
                    AddOrderBy(tokens, ref i, parent);
                    break;
                }
                else if (tokens[i].Type == TokenType.EndOfQuery || tokens[i].Type == TokenType.EndParenthesis)
                {
                    AddTokenSection(parent, tokens[i]);
                    break;
                }
                else if (IsQueryStart(tokens[i]))
                {
                    i--;
                    break;
                }

                AddTokenSection(section, tokens[i]);
            }
            return section;
        }

        //private SqlSection AddJoin(Token[] tokens, ref int i, List<SqlSection> result, SqlQuery parent)
        //{
        //    throw new NotImplementedException();
        //}

        private SqlWhere AddWhere(Token[] tokens, ref int i, SqlQuery parent)
        {
            var section = new SqlWhere(parent);
            section.Sections.Add(new SqlSection(section, new SqlToken(true, tokens[i])));
            parent.Sections.Add(section);

            for (i++; i < tokens.Length; i++)
            {
                if (IsMatch(tokens, i, "ORDER", "BY"))
                {
                    AddOrderBy(tokens, ref i, parent);
                    break;
                }
                else if (IsMatch(tokens, i, "GROUP", "BY"))
                {
                    AddGroupBy(tokens, ref i, parent);
                    break;
                }
                else if (tokens[i].Type == TokenType.StartParenthesis)
                {
                    if (IsMatch(tokens, i + 1, "SELECT"))
                    {
                        var query = new SqlQuery(section);
                        AddTokenSection(query, tokens[i]);
                        i++;
                        AddSelect(tokens, ref i, query);
                        section.Sections.Add(query);
                    }
                    else
                    {
                        AddParenthesisGroup(tokens, ref i, section);
                        continue;
                    }
                }
                else if (tokens[i].Type == TokenType.EndOfQuery || tokens[i].Type == TokenType.EndParenthesis)
                {
                    AddTokenSection(parent, tokens[i]);
                    break;
                }
                else if (IsQueryStart(tokens[i]))
                {
                    i--;
                    break;
                }

                AddTokenSection(section, tokens[i]);
            }
            return section;
        }

        private SqlSection AddParenthesisGroup(Token[] tokens, ref int i, SqlSection parent)
        {
            var section = new SqlSection(parent);
            section.Sections.Add(new SqlSection(section, new SqlToken(false, tokens[i])));
            parent.Sections.Add(section);

            for (i++; i < tokens.Length; i++)
            {
                if (tokens[i].Type == TokenType.StartParenthesis)
                {
                    if (IsMatch(tokens, i + 1, "SELECT"))
                    {
                        var query = new SqlQuery(section);
                        AddTokenSection(query, tokens[i]);
                        i++;
                        AddSelect(tokens, ref i, query);
                        section.Sections.Add(query);
                    }
                    else
                    {
                        AddParenthesisGroup(tokens, ref i, section);
                        continue;
                    }
                }
                else if (tokens[i].Type == TokenType.EndParenthesis)
                {
                    AddTokenSection(section, tokens[i]);
                    break;
                }
                else if (tokens[i].Type == TokenType.EndOfQuery)
                {
                    AddTokenSection(parent, tokens[i]);
                    break;
                }
                else if (IsQueryStart(tokens[i]))
                {
                    i--;
                    break;
                }

                AddTokenSection(section, tokens[i]);
            }
            return section;
        }

        private void AddUpdate(Token[] tokens, ref int i, List<SqlSection> result, SqlQuery parent)
        {
            throw new NotImplementedException();
        }

        private void AddTokenSection(SqlSection parent, Token t)
        {
            parent.AddTokenSection(IsReservedWord(t), t);
        }

        private int IsMatchWithLength(Token[] tokens, int i, string[] match)
        {
            if (match.Length > tokens.Length - i)
                return 0;

            // En typ unrollad variant för jämförelse av olika längd.
            switch (match.Length)
            {
                case 1:
                    return (tokens[i] == match[0]) ? 1 : 0;
                case 2:
                    return (tokens[i] == match[0] && tokens[i + 1] == match[1]) ? 2 : 0;
                case 3:
                    return (tokens[i] == match[0] && tokens[i + 1] == match[1] && tokens[i + 2] == match[2]) ? 3 : 0;
                case 4:
                    return (tokens[i] == match[0] && tokens[i + 1] == match[1] && tokens[i + 2] == match[2] && tokens[i + 3] == match[3]) ? 4 : 0;
            }

            throw new NotImplementedException("Har inte implementerat så lång jämförelse.");
        }

        private bool IsMatch(Token[] tokens, int i, params string[] match)
        {
            return IsMatchWithLength(tokens, i, match) > 0;
        }

        private bool IsMatch(Token[] tokens, int i, string match)
        {
            return IsMatch(tokens, i, match.Split(' '));
        }

        private bool IsReservedWord(Token token)
        {
            return ReservedWords.Contains(token.ToString());
        }

        private bool IsQueryStart(Token token)
        {
            return QueryStart.Contains(token.ToString());
        }
    }

    public class SqlSection
    {
        public SqlToken Token { get; private set; }
        public SqlSection Parent { get; private set; }
        public List<SqlSection> Sections { get; private set; }
        //public List<SqlToken> Tokens { get; private set; }
        
        public SqlSection(SqlSection parent)
        {
            Parent = parent;
            Sections = new List<SqlSection>();
        }

        public SqlSection(SqlSection parent, SqlToken token)
        {
            Parent = parent;
            Token = token;
        }

        public SqlSection AddTokenSection(bool isReservedWord, params Token[] tokens)
        {
            var section =  new SqlSection(this, new SqlToken(isReservedWord, tokens));
            Sections.Add(section);
            return section;
        }
    }

    public class SqlQuery : SqlSection
    {
        public SqlQuery(SqlSection parent)
            : base(parent)
        {
        }
    }

    public class SqlSelect : SqlSection
    {
        public SqlSelect(SqlSection parent)
	        : base(parent)
        {
	    }
    }

    public class SqlFrom : SqlSection
    {
        public SqlFrom(SqlSection parent)
	        : base(parent)
        {
	    }
    }

    public class SqlWhere : SqlSection
    {
        public SqlWhere(SqlSection parent)
            : base(parent)
        {
        }
    }

    public class SqlOrderBy : SqlSection
    {
        public SqlOrderBy(SqlSection parent)
            : base(parent)
        {
        }
    }

    public class SqlGroupBy : SqlSection
    {
        public SqlGroupBy(SqlSection parent)
            : base(parent)
        {
        }
    }

    public class SqlHaving : SqlSection
    {
        public SqlHaving(SqlSection parent)
            : base(parent)
        {
        }
    }

    //public class SqlSelect : SqlQuery
    //{
    //    public List<SqlArgument> Arguments { get; private set; }
    //    public SqlFrom From { get; set; }
    //    public SqlWhere Where { get; set; }
    //    public SqlOrderBy OrderBy { get; set; }
    //    public SqlGroupBy GroupBy { get; set; }
    //}

    //class SqlArgument : SqlToken
    //{
    //}

    //class SqlSelectArgument : SqlArgument
    //{
    //    public SqlSelect Select { get; set; }
    //}

    //class SqlColumnArgument : SqlArgument
    //{
    //    public SqlTable Table { get; set; }
    //    public SqlColumn Column { get; set; }
    //}

    //class SqlTable

    //class SqlEvaluationArgument : SqlArgument
    //{
    //}

    //public class SqlUpdate : SqlQuery
    //{
    //}

    //public class SqlInsert : SqlQuery
    //{
    //}

    //class SqlDeclaration : SqlQuery
    //{
    //}

    //class SqlFrom : SqlSection
    //{
    //    public SqlTableDeclaration Table { get; set; } 
    //    public List<SqlJoin> Joins { get; set; }
    //}

    //class SqlJoin : SqlSection
    //{
    //    public SqlToken Join { get; set; }
    //    public SqlTableDeclaration Table { get; set; }
    //    public SqlPredicate Predicate { get; set; }

    //    public bool IsInner { get; set; }
    //    public bool IsOuter { get { return !IsInner; } set { IsInner = !value; } }
    //    public bool IsLeft { get; set; }
    //    public bool IsRight { get { return !IsLeft; } set { IsLeft = !value; } }
    //}

    //class SqlPredicate : SqlSection
    //{
    //    public List<SqlEvaluation>
    //}

    //class SqlEvaluation : SqlSection
    //{
    //}

    //class SqlTableDeclaration : SqlSection
    //{
    //    public SqlToken Name { get; set; }
    //    public SqlToken Alias { get; set; }
    //}

    //class SqlWhere : SqlSection
    //{
    //}

    //class SqlOrderBy : SqlSection
    //{
    //}

    //class SqlGroupBy : SqlSection
    //{
    //}

    //select *, count(1) from Table where i < 0

    [DebuggerDisplay("[{ToString()}]")]
    public class SqlToken
    {
        public Token[] Tokens { get; private set; }
        public bool IsReserved { get; private set; }
        //public bool IsVariable { get; private set; }
        //public List<SqlToken> Children { get; private set; }
        //public bool HasChildren { get { return Children.Count > 0; } }

        public SqlToken(bool isReserved, params Token[] tokens)
        {
            //Children = new List<SqlToken>();
            Tokens = tokens;
            IsReserved = isReserved;
        }

        public override string ToString()
        {
            return string.Join(" ", Tokens.Select(x => x.ToString()));
        }

        public static bool operator ==(SqlToken x, string y)
        {
            return x.ToString() == y;
        }

        public static bool operator !=(SqlToken x, string y)
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

    //public enum SqlTokenType
    //{
    //    Block,
    //    Variable,
    //    SpecialVariable,
    //    StringLitteral,
    //    Operand,
    //    Table,
    //    Column
    //}
}
