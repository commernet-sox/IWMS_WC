using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.Core;
using iWMS.WebApplication;

namespace iWMS.WC2API.Models
{
    public class SRMOrderRequest : IEvent<Outcome>
    {
        /// <summary>
        /// 入库单号（唯一值）
        /// </summary>
        public string InputSerial { get; set; }
        /// <summary>
        /// 器具编码
        /// </summary>
        public string Appliance { get; set; }
        /// <summary>
        /// 投料点编码
        /// </summary>
        public string FeedingPointCode { get; set; }
        /// <summary>
        /// 投料点名称
        /// </summary>
        public string FeedingPointName { get; set; }
        /// <summary>
        /// 任务状态(1待入库2入库中3入库完成4拒收)
        /// </summary>
        public string Status { get; set; }
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
        public DateTime? PlanDate { get; set; }
        /// <summary>
        /// 波次
        /// </summary>
        public string WaveNum { get; set; }
        /// <summary>
        /// 批次
        /// </summary>
        public string BatchNum { get; set; }
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
        public long? Quantity { get; set; }
        /// <summary>
        /// 工位编码
        /// </summary>
        public string StationCode { get; set; }
        /// <summary>
        /// 工位名称
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// 拉动条码
        /// </summary>
        public string PullNumber { get; set; }
        /// <summary>
        /// 物料批次
        /// </summary>
        public string LotNumber { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string VendorNumber { get; set; }
        /// <summary>
        /// 器具类型编码
        /// </summary>
        public string UtensilTypeCode { get; set; }
        /// <summary>
        /// 入库单明细id
        /// </summary>
        public string DetailId { get; set; }

        /// <summary>
        /// 装车组编码
        /// </summary>
        public string CarGroupCode { get; set; }
        /// <summary>
        /// 装车组名称
        /// </summary>
        public string CarGroupName { get; set; }
        /// <summary>
        /// 产线波次
        /// </summary>
        public string LineWaveNum { get; set; }
        /// <summary>
        /// 包装顺序
        /// </summary>
        public long? PackSort { get; set; }
    }
}