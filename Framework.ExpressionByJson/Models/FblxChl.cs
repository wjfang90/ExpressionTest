using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.ExpressionByJson.Models
{
    public class FblxChl
    {
        public float TiaoNum { get; set; }
        public string UpdateTime { get; set; }
        public string KeyId { get; set; }
        public string Library { get; set; }
        public string UpdateType { get; set; }

        public string[] Category { get; set; }
    }
}
