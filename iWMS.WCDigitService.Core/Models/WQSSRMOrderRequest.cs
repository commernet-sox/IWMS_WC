using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCDigitService.Core.Models
{
    /// <summary>
    /// 01入库ASN同步
    /// </summary>
    public class WQSSRMOrderRequest
    {
        /// <summary>
        /// 批次号
        /// </summary>
        public string LotNumber { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemNumber { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemDesc { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal TransactionQuantity { get; set; }
        /// <summary>
        /// 库存组织
        /// </summary>
        public string OrganizationCode { get; set; }
        /// <summary>
        /// 供方编码
        /// </summary>
        public string VendorNum { get; set; }
        /// <summary>
        /// 供方名称
        /// </summary>
        public string VendorName { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime TransactionDate { get; set; }
        /// <summary>
        /// PO号
        /// </summary>
        public string PoNumber { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string PrimaryUomCode { get; set; }
        /// <summary>
        /// 待检（写死）
        /// </summary>
        public string StatusCode { get; set; }
        /// <summary>
        /// 货位
        /// </summary>
        public string LocatorCode { get; set; }
        /// <summary>
        /// 工序外（写死）
        /// </summary>
        public string ItemType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RcvTransactionId { get; set; }
        /// <summary>
        /// 子库
        /// </summary>
        public string SubinventoryCode { get; set; }
        /// <summary>
        /// 子库
        /// </summary>
        public string Attribute11 { get; set; }
        

    }
}
