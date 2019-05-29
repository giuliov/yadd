using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yadd.provider.mssql
{
    public class SqlServerScriptParser
    {
        public void ParseThis(string source)
        {
            var bootParser = new TSql80Parser(false);
            var parser = bootParser.Create(SqlVersion.Sql140, false);
            using (var s = new StringReader(source))
            {
                var script = parser.Parse(s, out IList<ParseError> errors) as TSqlScript;
                foreach (TSqlBatch batch in script.Batches)
                {
                    var visitor = new ReferencesVisitor();
                    batch.Accept(visitor);

                    foreach (SchemaObjectName reference in visitor.ReferencedObjects)
                    {
                        Debug.WriteLine(reference.BaseIdentifier.Value);
                    }
                }
            }
        }
    }

    class ReferencesVisitor : TSqlConcreteFragmentVisitor
    {
        public readonly IList<SchemaObjectName> ReferencedObjects = new List<SchemaObjectName>();

        public override void Visit(SchemaObjectName node)
        {
            ReferencedObjects.Add(node);
        }
    }

    class SelectVisitor : TSqlConcreteFragmentVisitor
    {
        public readonly IList<SelectStatement> SelectStatements = new List<SelectStatement>();

        public override void Visit(SelectStatement node)
        {
            SelectStatements.Add(node);
        }
    }
}
