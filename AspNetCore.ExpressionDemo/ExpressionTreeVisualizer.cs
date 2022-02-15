using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.ExpressionDemo
{
    /// <summary>
    /// 展示表达式树，协助用的
    /// 编译lambda--反编译C#--得到原始声明方式
    /// </summary>
    public class ExpressionTreeVisualizer
    {
        public static void Show()
        { 
            Expression<Func<int>> exp = () => 25 + 2; 
        }
    }
}
