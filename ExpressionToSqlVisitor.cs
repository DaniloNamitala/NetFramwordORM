using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DataSource
{
    public class ExpressionToSqlVisitor : ExpressionVisitor
    {
        private readonly StringBuilder _sql = new StringBuilder();

        public string Translate(Expression expression)
        {
            Visit(expression);
            return _sql.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _sql.Append("(");
            Visit(node.Left);
            _sql.Append($" {GetSqlOperator(node.NodeType)} ");
            Visit(node.Right);
            _sql.Append(")");
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            string col = node.Member.GetCustomAttribute<ColumnAttribute>().Name;
            _sql.Append(col); // Nome da coluna na SQL
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _sql.Append(node.Value is string ? $"'{node.Value}'" : node.Value);
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (new[] { "Contains", "StartsWith", "EndsWith" }.Contains(node.Method.Name) && node.Object != null)
            {
                Visit(node.Object); // Nome da coluna
                _sql.Append(" LIKE ");

                if (node.Arguments[0] is ConstantExpression constant)
                {
                    var value = constant.Value.ToString();
                    if (node.Method.Name == "Contains") 
                        _sql.Append($"'%{value}%'");
                    else if (node.Method.Name == "StartsWith")
                        _sql.Append($"'{value}%'");
                    else
                        _sql.Append($"'%{value}'");

                }
                else
                {
                    throw new NotSupportedException("Expressão complexa em Contains não suportada.");
                }

                return node;
            }

            throw new NotSupportedException($"O método '{node.Method.Name}' não é suportado.");
        }

        private string GetSqlOperator(ExpressionType nodeType) {
            switch (nodeType)
            {
                case ExpressionType.Equal: return "=";
                case ExpressionType.NotEqual: return "<>";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.AndAlso: return "AND";
                case ExpressionType.OrElse: return "OR";
                default: throw new NotSupportedException($"Operador {nodeType} não suportado.");
            }
        }
    }

}
