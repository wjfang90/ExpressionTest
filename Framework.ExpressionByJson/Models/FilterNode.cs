using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Framework.ExpressionByJson.Models
{
    public class DataFilterModel
    {
        /// <summary>
        /// 数据库名称
        /// </summary>
        public string LibraryName { get; set; }
        public FilterNode FilterNode { get; set; }
    }

    public class FilterNode
    {
        /// <summary>
        ///  数据库字段名
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 获取或设置规则集合ChildNodes之间使用的组合类型(初始化时指定值,如果使用的是无参构造方法则值是：And)
        /// </summary>
        public CombinationType CombinationType { get; set; }

        //
        // 摘要: 
        //     是否是多值字段
        public bool IsManyValue { get; set; }

        /// <summary>
        /// 运算符
        /// </summary>
        public RuleType RuleType { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        //
        // 摘要: 
        //     获取分配给当前规则树节点的 TreeRuleNode 对象的集合。 
        public List<FilterNode> ChildNodes { get; set; }

        public bool HasChild { get; set; }
    }

    public enum CombinationType
    {
        And = 1,
        Or = 2
    }

    public enum RuleType
    {
        Equal = 1,
        NotEqual = 2,
        GreaterThan = 3,
        GreaterThanOrEqual = 4,
        LessThan = 5,
        LessThanOrEqual = 6,
        Contains = 7,
        UnContains = 8,
        ListContains = 9,        
        DateTimeGreaterThan=10,
        DateTimeGreaterThanOrEqual = 11,
        DateTimeLessThan = 12,
        DateTimeLessThanOrEqual = 13,
    }
}
