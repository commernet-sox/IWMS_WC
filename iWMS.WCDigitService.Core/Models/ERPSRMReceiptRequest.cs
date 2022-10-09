using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCDigitService.Core.Models
{
    /// <summary>
    /// 02物料入库反馈（直投件）
    /// </summary>
    public class ERPSRMReceiptRequest
    {
        /// <summary>
        /// 数据唯一标识，无含义
        /// </summary>
        public long LineId { get; set; }
        /// <summary>
        /// 业务实体
        /// </summary>
        public string OrgId { get; set; }
        /// <summary>
        /// 库存组织
        /// </summary>
        public string OrganizationId { get; set; }
        /// <summary>
        /// 发运头表ID
        /// </summary>
        public string ShipmentHeaderId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SourceNumber { get; set; }
        /// <summary>
        /// ASN号
        /// </summary>
        public string ShipmentNum { get; set; }
        /// <summary>
        /// GMS系统事务处理编号,用于两个系统核对数据
        /// </summary>
        public string GmsNumber { get; set; }
        /// <summary>
        /// 采购订单号
        /// </summary>
        public string PoNumber { get; set; }
        /// <summary>
        /// 采购订单头表ID
        /// </summary>
        public string PoHeaderId { get; set; }
        /// <summary>
        /// 采购订单发放ID，PO_RELEASE_ID和RELEASE_NUMBER要么同为空，要么同不为空
        /// </summary>
        public string PoReleaseId { get; set; }
        /// <summary>
        /// 采购发放号
        /// </summary>
        public string ReleaseNumber { get; set; }
        /// <summary>
        /// 采购订单行表ID
        /// </summary>
        public string PoLineId { get; set; }
        /// <summary>
        /// 采购订单行号
        /// </summary>
        public string PoLineNumber { get; set; }
        /// <summary>
        /// 供应商ID
        /// </summary>
        public string VendorId { get; set; }
        /// <summary>
        /// 供方代码
        /// </summary>
        public string VendorsCode { get; set; }
        /// <summary>
        /// 供方名称
        /// </summary>
        public string VendorsName { get; set; }
        /// <summary>
        /// GMS传空值即可，ERP自用
        /// </summary>
        public string VendorsSite { get; set; }
        /// <summary>
        /// GMS传空值即可，ERP自用
        /// </summary>
        public string VendorSiteId { get; set; }
        /// <summary>
        /// 物料ID
        /// </summary>
        public string ItemId { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemDes { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 批次信息
        /// </summary>
        public string LotCode { get; set; }
        /// <summary>
        /// 接收数量
        /// </summary>
        public string ReceiptQuantity { get; set; }
        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTime ReceiptDate { get; set; }
        /// <summary>
        /// 接收子库
        /// </summary>
        public string SubinventoryName { get; set; }
        /// <summary>
        /// 货位ID
        /// </summary>
        public string LocatorId { get; set; }
        /// <summary>
        /// 货位编码
        /// </summary>
        public string LocatorName { get; set; }
        /// <summary>
        /// GMS最后更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; }
        /// <summary>
        /// GMS传空值即可，ERP自用，接收号
        /// </summary>
        public string ErpReceiptNumber { get; set; }
        /// <summary>
        /// ERP程序处理状态标识
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// ERP程序处理错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SourceId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LastUpdatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime LastUpdateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LastUpdateLogin { get; set; }
    }
}
