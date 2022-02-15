using Framework.ExpressionByJson.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Framework.ExpressionByJson.Extensions
{
    public static class ExpressionModelWhere
    {
        /// <summary>
        /// 根据json的配置条件转换为表达式树
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataLimitRange"></param>
        /// <param name="library"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GetListByWhere<T>(string dataLimitRange, string library)
        {
            if (string.IsNullOrEmpty(dataLimitRange) || string.IsNullOrEmpty(library))
            {
                return null;
            }
            List<DataFilterModel> dataFilters = JsonConvert.DeserializeObject<List<DataFilterModel>>(dataLimitRange);
            Expression<Func<T, bool>> rootExpression = t => true;

            var dataFilter = dataFilters.FirstOrDefault(t => t.LibraryName.Equals(library, StringComparison.CurrentCultureIgnoreCase));
            if (dataFilter != null)
            {
                if (!string.IsNullOrEmpty(dataFilter.FilterNode.FieldName) && !string.IsNullOrEmpty(dataFilter.FilterNode.Value))
                {
                    Expression<Func<T, bool>> nodeExpression = t => true;

                    if (dataFilter.FilterNode.IsManyValue)
                    {
                        var list = dataFilter.FilterNode.Value.Split(',');

                        var currentExpression = GetMethodCallExpression<T>(dataFilter.FilterNode, list[0]);
                        ParameterExpression newParameter = Expression.Parameter(typeof(T), "p");
                        var currentExp = Expression.Lambda<Func<T, bool>>(currentExpression, newParameter);

                        if (list.Count() > 1)
                        {
                            for (int i = 1; i < list.Length; i++)
                            {
                                var nextExpression = GetMethodCallExpression<T>(dataFilter.FilterNode, list[i]);
                                var nextExp = Expression.Lambda<Func<T, bool>>(nextExpression, newParameter);
                                nodeExpression = currentExp.Or<T>(nextExp);
                            }
                        }
                        else
                        {
                            nodeExpression = currentExp;
                        }
                    }
                    else
                    {
                        var currentExpression = GetMethodCallExpression<T>(dataFilter.FilterNode, dataFilter.FilterNode.Value);
                        ParameterExpression newParameter = Expression.Parameter(typeof(T), "p");

                        var currentExp = Expression.Lambda<Func<T, bool>>(currentExpression, newParameter);
                        nodeExpression = currentExp;
                    }

                    switch (dataFilter.FilterNode.CombinationType)
                    {
                        case CombinationType.And:
                        default:
                            rootExpression = rootExpression.And<T>(nodeExpression);
                            break;
                        case CombinationType.Or:
                            rootExpression = rootExpression.Or<T>(nodeExpression);
                            break;
                    }

                }

                if (dataFilter.FilterNode.HasChild)
                {
                    var resolverTreeNode = CreateExpressionTreeResolver<T>(dataFilter.FilterNode);

                    switch (dataFilter.FilterNode.CombinationType)
                    {
                        case CombinationType.And:
                        default:
                            rootExpression = rootExpression.And<T>(resolverTreeNode);
                            break;
                        case CombinationType.Or:
                            rootExpression = rootExpression.Or<T>(resolverTreeNode);
                            break;
                    }
                }
            }

            return rootExpression;
        }

        /// <summary>
        /// 递归创建表达式树
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterNode"></param>
        /// <returns></returns>
        private static Expression<Func<T, bool>> CreateExpressionTreeResolver<T>(FilterNode filterNode)
        {
            Expression<Func<T, bool>> filterNodeExpression = t => true;

            if (filterNode == null || filterNode.ChildNodes == null || filterNode.ChildNodes.Count == 0) return filterNodeExpression;

            foreach (var filterChildNode in filterNode.ChildNodes)
            {
                Expression<Func<T, bool>> nodeExpression = t => true;


                if (!string.IsNullOrEmpty(filterChildNode.FieldName) && !string.IsNullOrEmpty(filterChildNode.Value))
                {
                    if (filterChildNode.IsManyValue)
                    {
                        var list = filterChildNode.Value.Split(',');

                        var currentExpression = GetMethodCallExpression<T>(filterChildNode, list[0]);
                        ParameterExpression newParameter = Expression.Parameter(typeof(T), "p");
                        var currentExp = Expression.Lambda<Func<T, bool>>(currentExpression, newParameter);

                        if (list.Count() > 1)
                        {
                            for (int i = 1; i < list.Length; i++)
                            {
                                var nextExpression = GetMethodCallExpression<T>(filterChildNode, list[i]);
                                var nextExp = Expression.Lambda<Func<T, bool>>(nextExpression, newParameter);
                                nodeExpression = currentExp.Or<T>(nextExp);
                            }
                        }
                        else
                        {
                            nodeExpression = currentExp;
                        }
                    }
                    else
                    {
                        var currentExpression = GetMethodCallExpression<T>(filterChildNode, filterChildNode.Value);
                        ParameterExpression newParameter = Expression.Parameter(typeof(T), "p");
                        var currentExp = Expression.Lambda<Func<T, bool>>(currentExpression, newParameter);
                        nodeExpression = currentExp;
                    }
                }

                switch (filterNode.CombinationType)
                {
                    case CombinationType.And:
                    default:
                        filterNodeExpression = filterNodeExpression.And<T>(nodeExpression);

                        break;
                    case CombinationType.Or:
                        filterNodeExpression = filterNodeExpression.Or<T>(nodeExpression);
                        break;
                }

                if (filterChildNode.HasChild)//判断是否有子节点
                {
                    //递归调用
                    var resolverTreeNode = CreateExpressionTreeResolver<T>(filterChildNode);

                    switch (filterNode.CombinationType)
                    {
                        case CombinationType.And:
                        default:
                            filterNodeExpression = filterNodeExpression.And<T>(resolverTreeNode);
                            break;
                        case CombinationType.Or:
                            filterNodeExpression = filterNodeExpression.Or<T>(resolverTreeNode);
                            break;
                    }
                }
            }

            return filterNodeExpression;
        }

        private static MethodInfo GetMethodInfo(RuleType methodName)
        {
            MethodInfo methodInfo = null;

            switch (methodName)
            {
                case RuleType.Equal:
                    methodInfo = typeof(string).GetMethod(nameof(string.Equals), new Type[] { typeof(string) });
                    break;
                case RuleType.Contains:
                    methodInfo = typeof(string).GetMethod(nameof(string.Contains));
                    break;
                case RuleType.ListContains:
                    methodInfo = typeof(Enumerable).GetMethods().SingleOrDefault(t => t.Name == nameof(Enumerable.Contains) && t.GetParameters().Length == 2)?.MakeGenericMethod(typeof(string));
                    break;
                case RuleType.DateTimeGreaterThan:
                case RuleType.DateTimeGreaterThanOrEqual:
                case RuleType.DateTimeLessThan:
                case RuleType.DateTimeLessThanOrEqual:
                    methodInfo = typeof(DateTime).GetMethod(nameof(DateTime.CompareTo), new Type[] { typeof(DateTime) });
                    break;
                default:
                    methodInfo = null;
                    throw new Exception("不支持方法");
            }

            return methodInfo;
        }

        private static Expression GetMethodCallExpression<T>(FilterNode filterNode, string value)
        {
            var paraExpression = Expression.Parameter(typeof(T), "p");
            //FieldInfo fieldId = typeof(T).GetField(filterNode.FieldName);
            //MemberExpression exp = Expression.Field(paraExpression, fieldId);

            PropertyInfo propertyName = typeof(T).GetProperty(filterNode.FieldName);
            MemberExpression exp = Expression.Property(paraExpression, propertyName);
            //var expressionContant = Expression.Constant(value, typeof(string));

            ConstantExpression expressionContant = null;
            object constantValue = null;

            if (propertyName.PropertyType == typeof(string[]))
            {
                var dataElementType = typeof(string);
                //constantValue = value.Format(dataElementType);
                expressionContant = Expression.Constant(value, dataElementType);
            }
            else if (filterNode.RuleType.ToString().StartsWith("DateTime"))
            {
                //日期类型比较
                var dataElementType = typeof(DateTime);
                constantValue = value.Format(dataElementType);
                expressionContant = Expression.Constant(constantValue, dataElementType);
            }
            else
            {
                constantValue = value.Format(propertyName.PropertyType);
                expressionContant = Expression.Constant(constantValue, propertyName.PropertyType);
            }


            Expression result = null;

            switch (filterNode.RuleType)
            {
                case RuleType.Contains:
                case RuleType.Equal:
                    {
                        MethodInfo methodInfo = GetMethodInfo(filterNode.RuleType);
                        result = Expression.Call(exp, methodInfo, new Expression[] { expressionContant });
                        break;
                    }

                case RuleType.UnContains:
                case RuleType.NotEqual:
                    {
                        MethodInfo methodInfo = GetMethodInfo(filterNode.RuleType);
                        result = Expression.Call(exp, methodInfo, new Expression[] { expressionContant });
                        result = Expression.Not(result);
                        break;
                    }

                case RuleType.GreaterThan:
                    result = Expression.GreaterThan(exp, expressionContant);
                    break;
                case RuleType.GreaterThanOrEqual:
                    result = Expression.GreaterThanOrEqual(exp, expressionContant);
                    break;
                case RuleType.LessThan:
                    result = Expression.LessThan(exp, expressionContant);
                    break;
                case RuleType.LessThanOrEqual:
                    result = Expression.LessThanOrEqual(exp, expressionContant);
                    break;
                case RuleType.ListContains:
                    {
                        //方式1 ok
                        //result = Expression.Call(typeof(Enumerable), "Contains", new Type[] { typeof(string) }, exp, expressionContant);
                        MethodInfo methodInfo = GetMethodInfo(filterNode.RuleType);
                        result = Expression.Call(methodInfo, new Expression[] { exp, expressionContant });
                        break;
                    }
                case RuleType.DateTimeGreaterThan:
                    {
                        //Expression<Func<FblxChl, bool>> test = t => DateTime.Parse(t.UpdateTime).CompareTo(DateTime.Parse("2021.1.1")) > 0;
                        MethodCallExpression compareToExp = DateTimeCompareTo<T>(filterNode, expressionContant);
                        result = Expression.GreaterThan(compareToExp, Expression.Constant(0, typeof(int)));
                        break;
                    }
                case RuleType.DateTimeGreaterThanOrEqual:
                    {
                        //Expression<Func<FblxChl, bool>> test = t => DateTime.Parse(t.UpdateTime).CompareTo(DateTime.Parse("2021.1.1")) >= 0;
                        MethodCallExpression compareToExp = DateTimeCompareTo<T>(filterNode, expressionContant);
                        result = Expression.GreaterThanOrEqual(compareToExp, Expression.Constant(0, typeof(int)));
                        break;
                    }
                case RuleType.DateTimeLessThan:
                    {
                        //Expression<Func<FblxChl, bool>> test = t => DateTime.Parse(t.UpdateTime).CompareTo(DateTime.Parse("2021.1.1")) < 0;
                        MethodCallExpression compareToExp = DateTimeCompareTo<T>(filterNode, expressionContant);
                        result = Expression.LessThan(compareToExp, Expression.Constant(0, typeof(int)));
                        break;
                    }
                case RuleType.DateTimeLessThanOrEqual:
                    {
                        //Expression<Func<FblxChl, bool>> test = t => DateTime.Parse(t.UpdateTime).CompareTo(DateTime.Parse("2021.1.1")) <= 0;
                        MethodCallExpression compareToExp = DateTimeCompareTo<T>(filterNode, expressionContant);
                        result = Expression.LessThanOrEqual(compareToExp, Expression.Constant(0, typeof(int)));
                        break;
                    }
                default:
                    result = null;
                    break;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterNode">条件节点</param>
        /// <param name="expressionContant">比较值expression</param>
        /// <returns></returns>
        private static MethodCallExpression DateTimeCompareTo<T>(FilterNode filterNode, ConstantExpression expressionContant)
        {
            //Expression<Func<FblxChl,int>> test = t => DateTime.Parse(t.UpdateTime).CompareTo(DateTime.Parse("2021.1.1"));
            var parameter = Expression.Parameter(typeof(T), "t");
            PropertyInfo propertyName = typeof(T).GetProperty(filterNode.FieldName);
            var propertyExp = Expression.Property(parameter, propertyName);

            var dateTimeParseMethodInfo = typeof(DateTime).GetMethod("Parse", new Type[] { typeof(string) });
            var parsePropertyCallExp = Expression.Call(null, dateTimeParseMethodInfo, propertyExp);

            MethodInfo compareToMethodInfo = GetMethodInfo(filterNode.RuleType);

            var compareToExp = Expression.Call(parsePropertyCallExp, compareToMethodInfo, expressionContant);
            return compareToExp;
        }
    }
}
