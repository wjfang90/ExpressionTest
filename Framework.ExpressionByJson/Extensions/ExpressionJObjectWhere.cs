using Framework.ExpressionByJson.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Framework.ExpressionByJson.Extensions
{
    public class ExpressionJObjectWhere
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
                case RuleType.DateTimeGreaterThan:
                case RuleType.DateTimeGreaterThanOrEqual:
                case RuleType.DateTimeLessThan:
                case RuleType.DateTimeLessThanOrEqual:
                    methodInfo = typeof(DateTime).GetMethod(nameof(DateTime.CompareTo), new Type[] { typeof(DateTime) });
                    break;
                case RuleType.ListContains:
                    methodInfo = typeof(Enumerable).GetMethods().SingleOrDefault(t => t.Name == nameof(Enumerable.Contains) && t.GetParameters().Length == 2)?.MakeGenericMethod(typeof(string));

                    break;
                default:
                    methodInfo = null;
                    throw new Exception("不支持方法");
            }

            return methodInfo;
        }

        private static Expression GetMethodCallExpression<T>(FilterNode filterNode, string value)
        {
            //Func<JObject, bool> ff = t => t.ContainsKey("Title")
            //                              && t.GetValue("Title").ToObject<string>().Contains("fang")
            //                              && (t.GetValue("Category").GetType() == typeof(JArray) ? t.GetValue("Category").ToObject<string[]>().Contains("001") : t.GetValue("Category").ToObject<string>().Contains("001"))
            //                              && t.GetValue("UpdateTime").ToObject<DateTime>().CompareTo("2021.1.1 03:23:00") > 0;


            var paraExpression = Expression.Parameter(typeof(T), "p");
            var fieldNameExp = Expression.Constant(filterNode.FieldName, typeof(string));

            var containsKeyMethodInfo = typeof(T).GetMethod("ContainsKey");//JObject.ContainsKey
            var containsKeyCallExp = Expression.Call(paraExpression, containsKeyMethodInfo, fieldNameExp);


            var getValueMethodInfo = typeof(T).GetMethod("GetValue", new Type[] { typeof(string) });//JObject.GetValue
            var getValueCallExp = Expression.Call(paraExpression, getValueMethodInfo, fieldNameExp);


            Type dataElementType = null; //数据类型
            Type toObjectGenericType = null;//ToObject 泛型方法参数类型
            if (filterNode.RuleType.ToString().StartsWith("DateTime"))//日期类型
            {
                dataElementType = typeof(DateTime);
                toObjectGenericType = typeof(DateTime);
            }
            else if (filterNode.RuleType.ToString().Contains("Than"))//数字类型
            {
                dataElementType = typeof(float);
                toObjectGenericType = typeof(float);
            }
            else if (filterNode.RuleType == RuleType.ListContains)//字符数组类型
            {
                dataElementType = typeof(string);
                toObjectGenericType = typeof(string[]);
            }
            else
            {
                dataElementType = typeof(string);
                toObjectGenericType = typeof(string);
            }

            var constantValue = value.Format(dataElementType);
            ConstantExpression expressionContant = Expression.Constant(constantValue, dataElementType);

            //JToken.ToObject<T>
            var toObjectMethodInfo = typeof(JToken).GetMethod("ToObject", new Type[0]).MakeGenericMethod(new Type[] { toObjectGenericType });
            var exp = Expression.Call(getValueCallExp, toObjectMethodInfo);


            Expression ruleTypeExp = null;

            switch (filterNode.RuleType)
            {
                case RuleType.Equal:
                case RuleType.Contains:
                    {
                        MethodInfo methodInfo = GetMethodInfo(filterNode.RuleType);
                        ruleTypeExp = Expression.Call(exp, methodInfo, new Expression[] { expressionContant });
                        break;
                    }
                case RuleType.UnContains:
                case RuleType.NotEqual:
                    {
                        MethodInfo methodInfo = GetMethodInfo(filterNode.RuleType);
                        ruleTypeExp = Expression.Call(exp, methodInfo, new Expression[] { expressionContant });
                        ruleTypeExp = Expression.Not(ruleTypeExp);
                        break;
                    }
                case RuleType.GreaterThan:
                    {
                        ruleTypeExp = Expression.GreaterThan(exp, expressionContant);
                        break;
                    }
                case RuleType.GreaterThanOrEqual:
                    {
                        ruleTypeExp = Expression.GreaterThanOrEqual(exp, expressionContant);
                        break;
                    }
                case RuleType.LessThan:
                    {
                        ruleTypeExp = Expression.LessThan(exp, expressionContant);
                        break;
                    }
                case RuleType.LessThanOrEqual:
                    {
                        ruleTypeExp = Expression.LessThanOrEqual(exp, expressionContant);
                        break;
                    }
                case RuleType.ListContains:
                    {
                        //ok
                        //ruleTypeExp = Expression.Call(typeof(Enumerable), "Contains", new Type[] { typeof(string) }, exp, expressionContant);
                        MethodInfo methodInfo = GetMethodInfo(filterNode.RuleType);
                        ruleTypeExp = Expression.Call(methodInfo, exp, expressionContant);
                        break;
                    }
                case RuleType.DateTimeGreaterThan:
                    {
                        //Expression<Func<JObject, bool>> test = t => t.GetValue("UpdateTime").ToObject<DateTime>().CompareTo("2021.1.1 03:23:00") > 0;
                        MethodCallExpression compareToExp = DateTimeCompareTo(filterNode, exp, expressionContant);
                        ruleTypeExp = Expression.GreaterThan(compareToExp, Expression.Constant(0, typeof(int)));
                        break;
                    }
                case RuleType.DateTimeGreaterThanOrEqual:
                    {
                        //Expression<Func<JObject, bool>> test = t => t.GetValue("UpdateTime").ToObject<DateTime>().CompareTo("2021.1.1 03:23:00") >= 0;
                        MethodCallExpression compareToExp = DateTimeCompareTo(filterNode, exp, expressionContant);
                        ruleTypeExp = Expression.GreaterThanOrEqual(compareToExp, Expression.Constant(0, typeof(int)));
                        break;
                    }
                case RuleType.DateTimeLessThan:
                    {
                        //Expression<Func<JObject, bool>> test = t => t.GetValue("UpdateTime").ToObject<DateTime>().CompareTo("2021.1.1 03:23:00") < 0;
                        MethodCallExpression compareToExp = DateTimeCompareTo(filterNode, exp, expressionContant);
                        ruleTypeExp = Expression.LessThan(compareToExp, Expression.Constant(0, typeof(int)));
                        break;
                    }
                case RuleType.DateTimeLessThanOrEqual:
                    {
                        //Expression<Func<JObject, bool>> test = t => t.GetValue("UpdateTime").ToObject<DateTime>().CompareTo("2021.1.1 03:23:00") <= 0;
                        MethodCallExpression compareToExp = DateTimeCompareTo(filterNode, exp, expressionContant);
                        ruleTypeExp = Expression.LessThanOrEqual(compareToExp, Expression.Constant(0, typeof(int)));
                        break;
                    }
                default:
                    ruleTypeExp = null;
                    break;
            }

            var containsKeyFunc = Expression.Lambda<Func<T, bool>>(containsKeyCallExp, paraExpression);
            var ruleTypeFunc = Expression.Lambda<Func<T, bool>>(ruleTypeExp, paraExpression);
            var result = containsKeyFunc.And<T>(ruleTypeFunc);

            return result.Body;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterNode">条件节点</param>
        /// <param name="expressionContant">比较值expression</param>
        /// <returns></returns>
        private static MethodCallExpression DateTimeCompareTo(FilterNode filterNode, Expression paramExp, Expression expressionContant)
        {
            MethodInfo compareToMethodInfo = GetMethodInfo(filterNode.RuleType);
            var compareToExp = Expression.Call(paramExp, compareToMethodInfo, expressionContant);
            return compareToExp;
        }
    }
}
