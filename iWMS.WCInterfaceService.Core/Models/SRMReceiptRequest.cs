using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCInterfaceService.Core.Models
{
    public class SRMReceiptRequest
    {
        /// <summary>
        /// 产线编码
        /// </summary>
        public string ProductLineCode { get; set; }
        /// <summary>
        /// 产线名称
        /// </summary>
        public string ProductLineName { get; set; }
        /// <summary>
        /// 入库单号
        /// </summary>
        public string InputSerial { get; set; }
        /// <summary>
        /// 器具编码
        /// </summary>
        public string Appliance { get; set; }
        /// <summary>
        /// 计划日期
        /// </summary>
        public string PlanDate { get; set; }
        /// <summary>
        /// 拉动条码
        /// </summary>
        public string PullNumber { get; set; }
        /// <summary>
        /// 任务状态(1待入库2入库中3入库完成4拒收)
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 拒收原因
        /// </summary>
        public string NoMatchReason { get; set; }
        /// <summary>
        /// 接收人
        /// </summary>
        public string AcceptName { get; set; }
        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTime AcceptTime { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public long Quantity { get; set; }
        /// <summary>
        /// 仓库编码-收料仓库
        /// </summary>
        public string WarehouseCode { get; set; }
        /// <summary>
        /// 仓库名称-收料仓库
        /// </summary>
        public string WarehouseName { get; set; }
        /// <summary>
        /// 仓库编码-AGV缓存区
        /// </summary>
        public string AgvWarehouseCode { get; set; }
        /// <summary>
        /// 仓库名称-AGV缓存区
        /// </summary>
        public string AgvWarehouseName { get; set; }
        /// <summary>
        /// 入库单明细id
        /// </summary>
        //public string InputSerialDetail { get; set; }
        /// <summary>
        /// 入库单明细id
        /// </summary>
        public string DetailId { get; set; }
    }
}
