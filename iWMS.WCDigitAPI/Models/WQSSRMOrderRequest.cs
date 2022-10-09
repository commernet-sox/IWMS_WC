using iWMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iWMS.WCDigitAPI.Models
{
    /// <summary>
    /// 03 ASN质检结果下发
    /// </summary>
    public class WQSSRMOrderRequest:IEvent<Outcome>
    {
        /// <summary>
        /// 批次号
        /// </summary>
        public string SourceRecId { get; set; }
        /// <summary>
        /// 组织
        /// </summary>
        public string OrganizationCode { get; set; }
        /// <summary>
        /// 供方名称
        /// </summary>
        public string VendorName { get; set; }
        /// <summary>
        /// 供方编码
        /// </summary>
        public string VendorCode { get; set; }
        /// <summary>
        /// 物料种类
        /// </summary>
        public string ItemType { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemNumber { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemDescription { get; set; }
        /// <summary>
        /// 检验结果(有效/拒绝）
        /// </summary>
        public string InspectResult { get; set; }
        /// <summary>
        /// 检验时间
        /// </summary>
        public string InspectDate { get; set; }
    }
}