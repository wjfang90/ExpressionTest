using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.ExpressionByJson
{
    /// <summary>
    /// 行政处罚
    /// </summary>
    //[Library(Library = "APY", Describe = "行政处罚")]
    public class ApyModel
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 主题分类
        /// </summary>
        public string[] Category { get; set; }

        /// <summary>
        /// 发文字号
        /// </summary>
        public string DocumentNO { get; set; }


        /// <summary>
        /// 唯一标志
        /// </summary>
        public string Gid { get; set; }

        /// <summary>
        /// 全文
        /// </summary>
        public string FullText { get; set; }


        /// <summary>
        /// 没有链接的全文
        /// </summary>
        public string CheckFullText { get; set; }

        /// <summary>
        /// 实施机关
        /// </summary>
        public string[] ImplementOffice { get; set; }

        /// <summary>
        /// 处罚日期
        /// </summary>
        public string PunishmentDate { get; set; }

        /// <summary>
        /// 法学分类
        /// </summary>
        public string[] LawClassify { get; set; }

        /// <summary>
        /// 行业分类
        /// </summary>
        public string[] IndustryClassify { get; set; }

        /// <summary>
        /// 执法地域
        /// </summary>
        public string[] LawRegional { get; set; }

        /// <summary>
        /// 处罚依据
        /// </summary>
        public string[] AccordingLaw { get; set; }

        /// <summary>
        /// 处罚对象
        /// </summary>
        public string[] PunishmentObject { get; set; }

        /// <summary>
        /// 处罚种类
        /// </summary>
        public string[] PunishmentType { get; set; }

        /// <summary>
        /// 处罚对象（聚类用）
        /// </summary>
        public string[] PunishmentTarget { get; set; }

        /// <summary>
        /// 机关分类
        /// </summary>
        public string[] OrganClassify { get; set; }


        /// <summary>
        /// 执法级别
        /// </summary>
        public string EnforcementLevel { get; set; }


        /// <summary>
        /// 导航
        /// </summary>
        public string NavCatalog { get; set; }

        /// <summary>
        /// 法规引用量
        /// </summary>
        public int LawReferNum { get; set; }


        /// <summary>
        /// 案例引用量
        /// </summary>
        public int CaseReferNum { get; set; }

        /// <summary>
        /// 期刊引用量
        /// </summary>
        public int JournalReferNum { get; set; }

        /// <summary>
        /// 其他引用量
        /// </summary>
        public int OtherReferNum { get; set; }

        /// <summary>
        /// 引用的文章条
        /// </summary>
        public string[] ReferenceArticleGidTiaoNum { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public string UpdateTime { get; set; }

        /// <summary>
        /// 摘要
        /// </summary>
        public string ShowSummary { get; set; }


    }
}
