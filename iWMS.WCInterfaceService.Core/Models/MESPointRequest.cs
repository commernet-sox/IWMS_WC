using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCInterfaceService.Core.Models
{
    public class MESPointRequest
    {
        /// <summary>
        /// 投料单号-唯一值
        /// </summary>
        public string DeliverySerial { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 托盘码(器具码)
        /// </summary>
        public string Appliance { get; set; }
        /// <summary>
        /// 投料人
        /// </summary>
        public long CreateBy { get; set; }
        /// <summary>
        /// 投料时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 产线编码
        /// </summary>
        public string ProductLineCode { get; set; }
        /// <summary>
        /// 产线名称
        /// </summary>
        public string ProductLineName { get; set; }
        /// <summary>
        /// 计划日期
        /// </summary>
        public string PlanDate { get; set; }
        /// <summary>
        /// 投料方式
        /// </summary>
        public string FeedingMode { get; set; }

    }
}
