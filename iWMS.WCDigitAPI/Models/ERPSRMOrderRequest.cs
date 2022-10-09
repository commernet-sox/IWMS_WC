using iWMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iWMS.WCDigitAPI.Models
{
    public class ERPSRMOrderRequest : IEvent<Outcome>
    {
        public List<SRMOrder> PAsnHead { get; set; }
        public List<SRMOrderDetail> PAsnLine { get; set; }
    }

    public class SRMOrder
    {
        /// <summary>
        /// 业务实体ID
        /// </summary>
        public string OrgId { get; set; }
        /// <summary>
        /// 发运头表ID
        /// </summary>
        public string ShipmentHeaderId { get; set; }
        /// <summary>
        /// ASN号
        /// </summary>
        public string ShipmentNum { get; set; }
        /// <summary>
        /// 供方代码
        /// </summary>
        public string VendorsCode { get; set; }
        /// <summary>
        /// 供方名称
        /// </summary>
        public string VendorsName { get; set; }
        /// <summary>
        /// 采购订单头表ID
        /// </summary>
        public string PoheadId { get; set; }
        /// <summary>
        /// 采购发放ID
        /// </summary>
        public string PoreleaseId { get; set; }
        /// <summary>
        /// 采购订单号
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 采购发放号
        /// </summary>
        public string ReleaseNum { get; set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public string LastUpdateDate { get; set; }
        /// <summary>
        /// 到货时间
        /// </summary>
        public string ExpectedReceiptDate { get; set; }
    }

    public class SRMOrderDetail
    {
        /// <summary>
        /// 库存组织
        /// </summary>
        public string OrganizationId { get; set; }
        /// <summary>
        /// 发运头表ID
        /// </summary>
        public string ShipmentHeaderId { get; set; }
        /// <summary>
        /// ASN号
        /// </summary>
        public string ShipmentNum { get; set; }
        /// <summary>
        /// 采购订单头表ID
        /// </summary>
        public string PoheadId { get; set; }
        /// <summary>
        /// 采购发放ID
        /// </summary>
        public string PoreleaseId { get; set; }
        /// <summary>
        /// 采购订单号
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 采购发放号
        /// </summary>
        public string ReleaseNum { get; set; }
        /// <summary>
        /// 供方ID
        /// </summary>
        public string VendorsId { get; set; }
        /// <summary>
        /// 采购订单行表ID
        /// </summary>
        public string PolineId { get; set; }
        /// <summary>
        /// 采购订单行号
        /// </summary>
        public string OrderRowNum { get; set; }
        /// <summary>
        /// 物料ID
        /// </summary>
        public string ItemId { get; set; }
        /// <summary>
        /// 物料件号
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemDescription { get; set; }
        /// <summary>
        /// 主要单位
        /// </summary>
        public string Measure { get; set; }
        /// <summary>
        /// ASN检验标识（3-直接  2-检验  1-标准） 该值为2，需要检验
        /// </summary>
        public string ReceivingRoutingId { get; set; }
        /// <summary>
        /// 发运数量
        /// </summary>
        public string QuantityShipped { get; set; }
        /// <summary>
        /// 到货时间
        /// </summary>
        public string ExpectedReceiptDate { get; set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public string LastUpdateDate { get; set; }
    }
}