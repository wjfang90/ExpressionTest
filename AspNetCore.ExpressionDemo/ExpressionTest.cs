using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using System.Reflection;
using AspNetCore.ExpressionDemo.MappingExtend;

namespace AspNetCore.ExpressionDemo
{
    /// <summary>
    /// 认识/拼装 表达式目录树
    /// 拼装表达式
    /// 应用
    /// </summary>
    public class ExpressionTest
    {
        public static void Show()
        {
            {
                var query = new List<int>().AsQueryable();
                query = query.Where(c => c > 1);
                new List<int>().Where(i => i > 1);
            }
            {
                //Func<int, int, int> func = new Func<int, int, int>((m, n) => m * n + 2); 
                Func<int, int, int> func = (m, n) => m * n + 2;
                int iResult = func.Invoke(1, 2);

                //表达式目录树 展开以后，就好像对这个式子做了个描述；就像口语化的把这个式子给拆分了；
                //就可以在特定的场景下  拆分开各个小元素； 数据结构；
                Expression<Func<int, int, int>> exp = (m, n) => m * n + 2; //表达式目录树也可以通过拉姆达表达式来声明：快捷声明；
                var func1 = exp.Compile();
                int iResult1 = func1.Invoke(1, 2);

                //快捷声明表达式目录树的时候：只能有一行代码；不能带有大括号
                //Expression<Func<int, int, int>> exp1 = (m, n) =>
                //    {
                //        return m * n + 2;
                //    }; 
                Expression<Func<int, int, int>> exp1 = (m, n) => m * n + 2;

                int iResult3 = exp.Compile()(2, 3);
                //exp.Compile() 可以转换成一个委托
            }

            //那么他的真面目究竟是什么样的？
            {
                ///看他的IL  
                //Expression<Func<int>> exp = () => 25 + 2; 
                //Expression expressionContant25= Expression.Constant(25,typeof(int));
                //Expression expressionContant2 = Expression.Constant(2, typeof(int));
                //BinaryExpression binaryExpression= Expression.Add(expressionContant25, expressionContant2); 
                //Expression<Func<int>> expression = Expression.Lambda<Func<int>>(binaryExpression, Array.Empty<ParameterExpression>()); 
            }
            {
                //Expression<Func<int, int, int>> expNew = (m, n) => m * n + 2;

                //ParameterExpression parameterExpression = Expression.Parameter(typeof(int), "m");
                //ParameterExpression parameterExpression2 = Expression.Parameter(typeof(int), "n");

                //BinaryExpression multipley = Expression.Multiply(parameterExpression, parameterExpression2);

                //Expression expressionContant2 = Expression.Constant(2, typeof(int));
                //BinaryExpression binaryExpression = Expression.Add(multipley, expressionContant2);
                //Expression<Func<int, int, int>> expression = Expression.Lambda<Func<int, int, int>>(binaryExpression, new ParameterExpression[]
                //{
                //parameterExpression,
                //parameterExpression2
                //});
                //int iResult = expression.Compile().Invoke(10, 11);

            }
            //
            {
                //1.反编译
                //2.看中间语言
                // var peopleQuery=  new List<People>().AsQueryable(); 
                //Expression<Func<People, bool>> expression = p => p.Id.ToString().Equals("5");
                //peopleQuery.Where(expression); 
                ParameterExpression p = Expression.Parameter(typeof(People), "p");
                FieldInfo fieldId = typeof(People).GetField("Id");
                MemberExpression exp = Expression.Field(p, fieldId);
                MethodInfo toString = typeof(int).GetMethod("ToString",new Type[0]);  //为什么找不到？ 关于反射的内容 我们也有专门的课程讲解  大家可以添加课堂的助教老师的微信，可以获取关于反射的课程；
                var toStringExp = Expression.Call(exp, toString, Array.Empty<Expression>());
                MethodInfo equals = typeof(string).GetMethod("Equals",new Type[] { typeof(string) });
                Expression expressionContant = Expression.Constant("5");
                var equalsExp = Expression.Call(toStringExp, equals, new Expression[]
                {
                  expressionContant
                });
                Expression<Func<People, bool>> expression = Expression.Lambda<Func<People, bool>>(equalsExp, new ParameterExpression[]
                {
                p
                }); 
                bool bResult = expression.Compile().Invoke(new People() { Id = 5 });     
            } 
            { 
                People people=  new People();
                Expression<Func<People, bool>> expression = p => p.Id== people.Id;

                //课后如果你们在拼接表达式目录树的时候  可能会有问题；
                //强烈建议大家添加课堂的助教老师的微信，可以找到我；
            }
            #region 动态 
            {
                //拼接Sql
                {
                    //以前根据用户输入拼装条件
                    string sql = "SELECT * FROM USER WHERE 1=1";
                    Console.WriteLine("用户输入个名称，为空就跳过");
                    string name = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        sql += $" and name like '%{name}%'";
                    }

                    Console.WriteLine("用户输入个账号，为空就跳过");
                    string account = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(account))
                    {
                        sql += $" and account like '%{account}%'";
                    }
                }
                // 现在使用的LinqToSql； 
                Expression<Func<People, bool>> expression = null; 
                if (true) //xxx  id参数不为空
                {
                    expression = p => p.Id == 1;
                }
                if (true) //如果Name 不为空
                {
                    expression = p => p.Name == "";
                }
                if (true)
                { 
                }
                if (true)
                { 
                } 
                //怎么应该做？快捷声明 不好做！ 应该怎么做？ 如果徒手这样拼接 ，相对来说成本较高，应该封装一下；



            }
            #endregion
        }

        public static void MapperTest()
        {
            {
                People people = new People()
                {
                    Id = 11,
                    Name = "朝夕Richard老师",
                    Age = 31
                };
                //PeopleCopy copy = (PeopleCopy)people; 
                PeopleCopy peopleCopy = new PeopleCopy()
                {
                    Id = people.Id,
                    Name = people.Name,
                    Age = people.Age
                };
            }


            #region 性能测试
            {
                People people = new People()
                {
                    Id = 11,
                    Name = "朝夕教育Richard老师",
                    Age = 31
                };
                long common = 0;
                long generic = 0;
                long cache = 0;
                long reflection = 0;
                long serialize = 0;
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    for (int i = 0; i < 1_000_000; i++)
                    {
                        PeopleCopy peopleCopy = new PeopleCopy()
                        {
                            Id = people.Id,
                            Name = people.Name,
                            Age = people.Age
                        };
                    }
                    watch.Stop();
                    common = watch.ElapsedMilliseconds;
                }
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    for (int i = 0; i < 1_000_000; i++)
                    {
                        PeopleCopy peopleCopy = ReflectionMapper.Trans<People, PeopleCopy>(people);
                    }
                    watch.Stop();
                    reflection = watch.ElapsedMilliseconds;
                }
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    for (int i = 0; i < 1_000_000; i++)
                    {
                        PeopleCopy peopleCopy = SerializeMapper.Trans<People, PeopleCopy>(people);
                    }
                    watch.Stop();
                    serialize = watch.ElapsedMilliseconds;
                }
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    for (int i = 0; i < 1_000_000; i++)
                    {
                        PeopleCopy peopleCopy = ExpressionMapper.Trans<People, PeopleCopy>(people);
                    }
                    watch.Stop();
                    cache = watch.ElapsedMilliseconds;
                }
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    for (int i = 0; i < 1_000_000; i++)
                    {
                        PeopleCopy peopleCopy = ExpressionGenericMapper<People, PeopleCopy>.Trans(people);
                    }
                    watch.Stop();
                    generic = watch.ElapsedMilliseconds;
                }
                Console.WriteLine($"common = { common} ms");
                Console.WriteLine($"reflection = { reflection} ms");
                Console.WriteLine($"serialize = { serialize} ms");
                Console.WriteLine($"cache = { cache} ms");
                Console.WriteLine($"generic = { generic} ms");
            }
            #endregion 
        }


    }
}
