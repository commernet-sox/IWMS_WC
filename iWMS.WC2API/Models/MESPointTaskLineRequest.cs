using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.Core;
using iWMS.WebApplication;

namespace iWMS.WC2API.Models
{
    public class MESPointTaskLineRequest:IEvent<Outcome>
    {
        /// <summary>
        /// 投料单号
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
        /// 拉动条码
        /// </summary>
        public string PullNumber { get; set; }
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
        public string NeedNum { get; set; }
        /// <summary>
        /// 装车组编码
        /// </summary>
        public string CarGroupCode { get; set; }
        /// <summary>
        /// 装车组名称
        /// </summary>
        public string CarGroupName { get; set; }
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
        /// 投料方式
        /// </summary>
        public string FeedingMode { get; set; }
        /// <summary>
        /// 工位编码
        /// </summary>
        public string StationCode { get; set; }
        /// <summary>
        /// 工位名称
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// 波次
        /// </summary>
        public string WaveNum { get; set; }
        /// <summary>
        /// 批次
        /// </summary>
        public string BatchNum { get; set; }
        /// <summary>
        /// 投料点编码
        /// </summary>
        public string FeedingPointCode { get; set; }
        /// <summary>
        /// 投料点名称
        /// </summary>
        public string FeedingPointName { get; set; }
        /// <summary>
        /// 器具类型编码
        /// </summary>
        public string UtensilTypeCode { get; set; }
        /// <summary>
        /// 停靠顺序
        /// </summary>
        public string DockingOrder { get; set; }
        /// <summary>
        /// 停靠点
        /// </summary>
        public string StopPointName { get; set; }
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