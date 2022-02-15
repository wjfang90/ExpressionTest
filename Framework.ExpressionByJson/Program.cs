using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Framework.ExpressionByJson.Models;
using System.Linq.Expressions;
using System.Reflection;
using Framework.ExpressionByJson.Extensions;
using Newtonsoft.Json.Linq;

namespace Framework.ExpressionByJson
{
    class Program
    {
        static void Main(string[] args)
        {
            #region expression 反编译
            //refrector framework版本 表达式树反编译代码如下

            /*
			Expression<Func<ApyModel, bool>> test = t => t.ImplementOffice.Contains("test");

			ParameterExpression parameter;
			parameter = Expression.Parameter(typeof(ApyModel), "t");
			var value = Expression.Constant("test", typeof(string));

			var property = Expression.Property(parameter, (MethodInfo)methodof(ApyModel.get_ImplementOffice));
			
			Expression[] arguments = new Expression[] { property, value };

			var exp = Expression.Call(null, (MethodInfo)methodof(Enumerable.Contains), arguments);

		    ParameterExpression[] parameters = new ParameterExpression[] { parameter };
			Expression<Func<ApyModel, bool>> test = Expression.Lambda<Func<ApyModel, bool>>(exp, parameters);

			*/

            /*
            Expression<Func<FblxChl, bool>> test = t => DateTime.Parse(t.UpdateTime).CompareTo(DateTime.Parse("2021.1.1")) > 0;
            
            //refrector framework 表达式树反编译代码如下
            
            ParameterExpression expression;
            Expression[] arguments = new Expression[] { Expression.Property(expression = Expression.Parameter(typeof(FblxChl), "t"), (MethodInfo)methodof(FblxChl.get_UpdateTime)) };
            Expression[] expressionArray2 = new Expression[1];
            Expression[] expressionArray3 = new Expression[] { Expression.Constant("2021.1.1", typeof(string)) };
            expressionArray2[0] = Expression.Call(null, (MethodInfo)methodof(DateTime.Parse), expressionArray3);

            ParameterExpression[] parameters = new ParameterExpression[] { expression };

            var parseExp = Expression.Call(null, (MethodInfo)methodof(DateTime.Parse), arguments);

            var compareToExp = Expression.Call(parseExp, (MethodInfo)methodof(DateTime.CompareTo), expressionArray2);

            var exp = Expression.GreaterThan(compareToExp, Expression.Constant(0, typeof(int)));

            Expression<Func<FblxChl, bool>> test = Expression.Lambda<Func<FblxChl, bool>>(exp, parameters);

            */


            //使用Newtonsoft.Json.Linq.JObject 序列化json数据
            /*
           Func<JObject, bool> ff = t => t.ContainsKey("Title") && t.GetValue("Title").HasValues
                                          && t.GetValue("Title").ToObject<string>().Contains("fang")
                                          && (t.GetValue("Category").GetType() == typeof(JArray) ? t.GetValue("Category").ToObject<string[]>().Contains("001") : t.GetValue("Category").ToObject<string>().Contains("001"))
                                          && t.GetValue("Category").ToObject<DateTime>().CompareTo("2021.1.1 03:23:00") > 0;
           */
            #endregion


            var singleOrDefault = typeof(Queryable).GetMethodWithLinq("SingleOrDefault", typeof(IQueryable<>), typeof(Expression<>)).MakeGenericMethod(typeof(string));
            var count = typeof(Enumerable).GetMethodWithLinq("Count", typeof(IEnumerable<>)).MakeGenericMethod(typeof(string));
            //var contains = typeof(Enumerable).GetMethodWithLinq("Contains", typeof(IEnumerable<>), typeof(string)).MakeGenericMethod(typeof(string));//泛型重载方法 error

            //泛型重载方法 ok
            var method = typeof(Enumerable).GetMethods()
                                            .Where(m => m.Name == nameof(Enumerable.Contains))
                                            .SingleOrDefault(m => m.GetParameters().Length == 2)
                                            ?.MakeGenericMethod(typeof(string));

            TestByJObject("Load_fblxchl", "fblxchl");

            TestListByModel<FblxChl>("Load_fblxchl", "fblxchl");
            TestListByModel<ApyModel>("Load_apy", "apy");

            Console.ReadKey();
        }

        /// <summary>
        /// 使用Newtonsoft.Json.Linq.JObject 序列化json数据
        /// </summary>
        /// <param name="jsonDataFileName"></param>
        /// <param name="library"></param>
        private static void TestByJObject(string jsonDataFileName, string library)
        {
            //加载数据
            var jsonPath = Path.Combine(AppContext.BaseDirectory, $"data/{jsonDataFileName}.json");
            var jsonContent = File.ReadAllText(jsonPath, Encoding.UTF8);
            var jsonObj = JsonConvert.DeserializeObject<List<Newtonsoft.Json.Linq.JObject>>(jsonContent);

            //加载条件
            var whereJsonPath = Path.Combine(AppContext.BaseDirectory, "config/where.json");
            var whereJson = File.ReadAllText(whereJsonPath, Encoding.UTF8);

            try
            {
                var exp = ExpressionJObjectWhere.GetListByWhere<Newtonsoft.Json.Linq.JObject>(whereJson, library);
                var result = jsonObj.Where(exp.Compile());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根据配置文件中的复合条件过滤集合数据功能
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonDataFileName"></param>
        /// <param name="library"></param>
        private static void TestListByModel<T>(string jsonDataFileName, string library)
        {
            //加载数据
            var jsonPath = Path.Combine(AppContext.BaseDirectory, $"data/{jsonDataFileName}.json");
            var jsonContent = File.ReadAllText(jsonPath, Encoding.UTF8);
            var jsonObj = JsonConvert.DeserializeObject<List<T>>(jsonContent);

            //加载条件
            var whereJsonPath = Path.Combine(AppContext.BaseDirectory, "config/where.json");
            var whereJson = File.ReadAllText(whereJsonPath, Encoding.UTF8);

            //生成复合检索条件 json 条件转换成lambda条件
            var exp = ExpressionModelWhere.GetListByWhere<T>(whereJson, library);
            var result = jsonObj.Where(exp.Compile());
        }
    }

    public static class MethodInfoExtension
    {
        /// <summary>
        /// 获取MethodIfo
        /// 不支持泛型重载方法
        /// </summary>
        /// <param name="staticType"></param>
        /// <param name="methodName"></param>
        /// <param name="paramTypes"></param>
        /// <returns></returns>
        public static MethodInfo GetMethodWithLinq(this Type staticType, string methodName, params Type[] paramTypes)
        {
            var methods = from method in staticType.GetMethods()
                          where method.Name == methodName
                                && method.GetParameters()
                                         .Select(parameter => parameter.ParameterType)
                                         .Select(type => type.IsGenericType ?
                                             type.GetGenericTypeDefinition() : type)
                                         .SequenceEqual(paramTypes)
                          select method;
            try
            {
                return methods.SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new AmbiguousMatchException();
            }
        }
    }


}
