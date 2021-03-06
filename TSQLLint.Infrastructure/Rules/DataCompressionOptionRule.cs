using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using TSQLLint.Core.Interfaces;

namespace TSQLLint.Infrastructure.Rules
{
    public class DataCompressionOptionRule : TSqlFragmentVisitor, ISqlRule
    {
        private readonly Action<string, string, int, int> errorCallback;

        public DataCompressionOptionRule(Action<string, string, int, int> errorCallback)
        {
            this.errorCallback = errorCallback;
        }

        public string RULE_NAME => "data-compression";

        public string RULE_TEXT => "Expected table to use data compression";

        public int DynamicSqlStartColumn { get; set; }

        public int DynamicSqlStartLine { get; set; }

        public override void Visit(CreateTableStatement node)
        {
            var childCompressionVisitor = new ChildCompressionVisitor();
            node.AcceptChildren(childCompressionVisitor);

            if (!childCompressionVisitor.CompressionOptionExists)
            {
                errorCallback(RULE_NAME, RULE_TEXT, node.StartLine, GetColumnNumber(node));
            }
        }

        private int GetColumnNumber(TSqlFragment node)
        {
            return node.StartLine == DynamicSqlStartLine
                ? node.StartColumn + DynamicSqlStartColumn
                : node.StartColumn;
        }

        private class ChildCompressionVisitor : TSqlFragmentVisitor
        {
            public bool CompressionOptionExists
            {
                get;
                private set;
            }

            public override void Visit(DataCompressionOption node)
            {
                CompressionOptionExists = true;
            }
        }
    }
}
