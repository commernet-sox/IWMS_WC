using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.Core;
using iWMS.WebApplication;

namespace iWMS.WC2API.Models
{
    public class MESProductLineRequest : IEvent<Outcome>
    {
        /// <summary>
        /// 产线编码（唯一）
        /// </summary>
        public string ProductlineCode { get; set; }
        /// <summary>
        /// 产线名称
        /// </summary>
        public string ProductlineName { get; set; }
        /// <summary>
        /// 汇总波次
        /// </summary>
        public int? BatchNum { get; set; }
        /// <summary>
        /// 投料台数
        /// </summary>
        public int? FeedingNumber { get; set; }
    }
}