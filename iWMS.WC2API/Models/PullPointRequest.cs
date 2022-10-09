using iWMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.WebApplication;

namespace iWMS.WC2API.Models
{
    public class PullPointRequest : IEvent<Outcome>
    {
        /// <summary>
        /// 任务号
        /// </summary>
        public string TaskId { get; set; }
        /// <summary>
        /// 发动机序列号
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 产线编码
        /// </summary>
        public string ProductLineCode { get; set; }
        /// <summary>
        /// 工位编码
        /// </summary>
        public string OperationCode { get; set; }
        /// <summary>
        /// 过站时间
        /// </summary>
        public DateTime StationTime { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }
    }
}