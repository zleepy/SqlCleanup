using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlCleanup.Parser;

internal sealed partial class sql1
{
    //private ObjectExpression DoCreateObjectReference(List<Result> results)
    //{
    //    switch (results.Count(x => x.Text == "."))
    //    {
    //        case 3:
    //            // server.database.schema.col
    //            // server..schema.col
    //            // server.database..col
    //            // server...col
    //            {
    //                string database = results[2].Text == "." ? null : results[2].Text;
    //                int nextResultIndex = database == null ? 3 : 4;
    //                string schema = results[nextResultIndex].Text == "." ? null : results[nextResultIndex].Text;
    //                nextResultIndex = schema == null ? nextResultIndex + 1 : nextResultIndex + 2;

    //                return new ObjectExpression(results[0].Text, database, schema, results[nextResultIndex].Text);
    //            }

    //        case 2:
    //            // database.schema.col
    //            // database..col

    //            {
    //                string schema = results[2].Text == "." ? null : results[2].Text;
    //                int nextResultIndex = schema == null ? 3 : 4;

    //                return new ObjectExpression(results[0].Text, schema, results[nextResultIndex].Text);
    //            }

    //        case 1:
    //            // schema.col
    //            return new ObjectExpression(results[0].Text, results[2].Text);

    //        case 0:
    //            // col
    //            return new ObjectExpression(results[0].Text);

    //        default:
    //            throw new ArgumentException("Ogiltiga argument till DoCreateObjectReference.");
    //    }
    //}

    //private SelectExpression DoCreateSelect(List<Result> results)
    //{
    //    bool isDistinct = (results.Count >= 2) ? (results[1].Text == "distinct") : false;
    //    bool isAll = (results.Count >= 2) ? (results[1].Text == "all") : false;

    //    return new SelectExpression(results.Where(r => r.Value is AliasStatementExpression).Select(r => (AliasStatementExpression)r.Value), isDistinct, isAll);
    //}

    //private AliasStatementExpression DoCreateAliasStatement(List<Result> results)
    //{
    //    if (results.Count == 3)
    //        return new AliasStatementExpression(results[0].Value ?? new Expression(results[0].Text), results[2].Value, results[1].Text.ToLower() == "as");

    //    if (results.Count == 1)
    //        return new AliasStatementExpression(results[0].Value ?? new Expression(results[0].Text));

    //    throw new ArgumentException("Borde inte komma hit!");
    //}
}

namespace SqlCleanup.Parser
{
    class Expression
    {
        public virtual string Text { get; protected set; }

        public Expression()
        {
        }

        public Expression(string text)
        {
            Text = text;
        }
    }

    class ObjectExpression : Expression
    {
        public string Server { get; private set; }
        public string Database { get; private set; }
        public string Schema { get; private set; }
        public string Object { get; private set; }

        public ObjectExpression(string server, string database, string schema, string obj)
            : base(server + "." + (database ?? "") + "." + (schema ?? "") + "." + obj)
        {
            Server = server;
            Database = database;
            Schema = schema;
            Object = obj;
        }

        public ObjectExpression(string database, string schema, string obj)
            : base(database + "." + (schema ?? "") + "." + obj)
        {
            Server = null;
            Database = database;
            Schema = schema;
            Object = obj;
        }

        public ObjectExpression(string schema, string obj)
            : base(schema + "." + obj)
        {
            Server = null;
            Database = null;
            Schema = schema;
            Object = obj;
        }

        public ObjectExpression(string obj)
            : base(obj)
        {
            Server = null;
            Database = null;
            Schema = null;
            Object = obj;
        }
    }

    class AliasStatementExpression : Expression
    {
        public Expression Expression { get; set; }
        public bool IsAsDefined { get; set; }
        public Expression Identifier { get; set; }

        public AliasStatementExpression(Expression expression)
            : base(expression.Text)
        {
            Expression = expression;
        }

        public AliasStatementExpression(Expression expression, Expression identifier, bool isAsDefined)
            : base(expression.Text)
        {
            Expression = expression;
            Identifier = identifier;
            IsAsDefined = isAsDefined;
        }
    }

    class SelectExpression : Expression
    {
        public bool IsDistinct { get; set; }
        public bool IsAll { get; set; }

        public AliasStatementExpression[] Statements { get; set; }

        public SelectExpression(IEnumerable<AliasStatementExpression> statements, bool isDistinct, bool isAll)
        {
            Statements = statements.ToArray();
            IsDistinct = isDistinct;
            IsAll = isAll;
        }
    }

    class LitteralExpression : Expression
    {
        public bool IsString { get; private set; }
        public bool IsNumber { get; private set; }

        public LitteralExpression(string litteral, bool isString)
            : base(litteral)
        {
            IsString = isString;
            IsNumber = !isString;
        }
    }

    class IdentifierExpression : Expression
    {
        public IdentifierExpression(string litteral)
            : base(litteral)
        {
        }
    }
}
