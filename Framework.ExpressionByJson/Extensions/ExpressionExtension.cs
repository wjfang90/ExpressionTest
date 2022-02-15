using Framework.ExpressionByJson.Vistor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Framework.ExpressionByJson.Extensions
{
    /// <summary>
    /// 合并表达式 And Or Not扩展方法
    /// </summary>
    public static class ExpressionExtension
    {
        /// <summary>
        /// 合并表达式 expr1 AND expr2
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr1"></param>
        /// <param name="expr2"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            if (expr1 == null || expr2 == null)
            {
                throw new Exception("null不能处理");
            }
            ParameterExpression newParameter = Expression.Parameter(typeof(T), "x");
            NewExpressionVisitor visitor = new NewExpressionVisitor(newParameter);
            Expression left = visitor.Visit(expr1.Body);
            Expression right = visitor.Visit(expr2.Body);
            BinaryExpression body = Expression.And(left, right);
            return Expression.Lambda<Func<T, bool>>(body, newParameter);
        }

        /// <summary>
        /// 合并表达式 expr1 AND expr2
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr1"></param>
        /// <param name="expr2"></param>
        /// <returns></returns>
        public static Expression<Func<T1, bool>> And<T1,T2>(this Expression<Func<T1, bool>> expr1, Expression<Func<T2, bool>> expr2)
        {
            if (expr1 == null || expr2 == null)
            {
                throw new Exception("null不能处理");
            }
            ParameterExpression newParameter1 = Expression.Parameter(typeof(T1), "x");
            NewExpressionVisitor visitor1 = new NewExpressionVisitor(newParameter1);

            ParameterExpression newParameter2 = Expression.Parameter(typeof(T2), "x");
            NewExpressionVisitor visitor2 = new NewExpressionVisitor(newParameter2);

            Expression left = visitor1.Visit(expr1.Body);
            Expression right = visitor2.Visit(expr2.Body);
            BinaryExpression body = Expression.And(left, right);
            return Expression.Lambda<Func<T1, bool>>(body, newParameter1, newParameter2);
        }

        /// <summary>
        /// 合并表达式 expr1 or expr2
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr1"></param>
        /// <param name="expr2"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            if (expr1 == null || expr2 == null)
            {
                throw new Exception("null不能处理");
            }
            ParameterExpression newParameter = Expression.Parameter(typeof(T), "x");
            NewExpressionVisitor visitor = new NewExpressionVisitor(newParameter);
            Expression left = visitor.Visit(expr1.Body);
            Expression right = visitor.Visit(expr2.Body);
            BinaryExpression body = Expression.Or(left, right);
            return Expression.Lambda<Func<T, bool>>(body, newParameter);
        }

        public static Expression<Func<T1, bool>> Or<T1, T2>(this Expression<Func<T1, bool>> expr1, Expression<Func<T2, bool>> expr2)
        {
            if (expr1 == null || expr2 == null)
            {
                throw new Exception("null不能处理");
            }
            ParameterExpression newParameter1 = Expression.Parameter(typeof(T1), "x");
            NewExpressionVisitor visitor1 = new NewExpressionVisitor(newParameter1);

            ParameterExpression newParameter2 = Expression.Parameter(typeof(T2), "x");
            NewExpressionVisitor visitor2 = new NewExpressionVisitor(newParameter2);

            Expression left = visitor1.Visit(expr1.Body);
            Expression right = visitor2.Visit(expr2.Body);
            BinaryExpression body = Expression.Or(left, right);
            return Expression.Lambda<Func<T1, bool>>(body, newParameter1, newParameter2);
        }

        /// <summary>
        /// 表达式取非
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expr)
        {
            if (expr == null)
            {
                throw new Exception("null不能处理");
            }
            ParameterExpression newParameter = expr.Parameters[0];
            UnaryExpression body = Expression.Not(expr.Body);
            return Expression.Lambda<Func<T, bool>>(body, newParameter);
        }
    }
}
