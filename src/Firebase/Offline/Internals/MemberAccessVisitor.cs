namespace Firebase.Database.Offline.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    public class MemberAccessVisitor : ExpressionVisitor
    {
        private readonly IList<string> propertyNames = new List<string>();

        public IEnumerable<string> PropertyNames => this.propertyNames;

        public MemberAccessVisitor()
        {
        }

        public override Expression Visit(Expression expr)
        {
            if (expr?.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = (MemberExpression)expr;
                this.propertyNames.Add(memberExpr.Member.Name);
            }
            else if (expr?.NodeType == ExpressionType.Call)
            {
                var callExpr = (MethodCallExpression)expr;
                if (callExpr.Method.Name == "get_Item" && callExpr.Arguments.Count == 1)
                {
                    var e = Expression.Lambda(callExpr.Arguments[0]).Compile();
                    this.propertyNames.Add(e.DynamicInvoke().ToString());
                }
            }

            return base.Visit(expr);
        }
    }
}
