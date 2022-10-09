using iWMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.WebApplication;

namespace iWMS.WC2API.Models
{
    public class MESProducePlanRequest : IEvent<Outcome>
    {
        /// <summary>
        /// BOM头id
        /// </summary>
        public string BomHeaderId { get; set; }
        /// <summary>
        /// 工单号ID
        /// </summary>
        public string WipEntityId { get; set; }
        /// <summary>
        /// 工单编码
        /// </summary>
        public string WipEntityName { get; set; }
        /// <summary>
        /// 计划顺序
        /// </summary>
        public string MesPlanOrder { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 机型
        /// </summary>
        public string ProductType { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// ERP计划开始时间
        /// </summary>
        public DateTime? ErpStartDate { get; set; }
        /// <summary>
        /// 3为锁定，1为未锁定
        /// </summary>
        public string IsLocked { get; set; }
    }
}