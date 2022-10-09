using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCDigitService.Core.Models
{
    /// <summary>
    /// 投料任务回传反馈
    /// </summary>
    public class MESPointBackRequest
    {
        /// <summary>
        /// 出库单号
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// 拉动条码
        /// </summary>
        public string PullNumber { get; set; }
        /// <summary>
        /// 产线编码
        /// </summary>
        public string ProductLineCode { get; set; }
        /// <summary>
        /// 货主
        /// </summary>
        public string ItemOwner { get; set; }
        /// <summary>
        /// 供应商
        /// </summary>
        public string VendorCode { get; set; }
        /// <summary>
        /// 物料
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 批次
        /// </summary>
        public string Batch { get; set; }
        /// <summary>
        /// 物料数量
        /// </summary>
        public string Quantity { get; set; }
        /// <summary>
        /// 保管员
        /// </summary>
        public string SortCustodian { get; set; }
        /// <summary>
        /// 来源子库存
        /// </summary>
        public string FromSubinventory { get; set; }
        /// <summary>
        /// 接收目标组织ID
        /// </summary>
        public string ToOrganizationId { get; set; }
        /// <summary>
        /// 接收目标组织代码
        /// </summary>
        public string ToOrganizationCode { get; set; }
        /// <summary>
        /// 接收目标子库存
        /// </summary>
        public string ToSubinventory { get; set; }
        /// <summary>
        /// 来源系统
        /// </summary>
        public string SourceCode { get; set; }
        /// <summary>
        /// 接收人
        /// </summary>
        public string Receiver { get; set; }

    }
}
