using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCInterfaceService.Core.Models
{
    public class MESPullTaskRequest
    {
        /// <summary>
        /// 计划日期
        /// </summary>
        public string PlanDate { get; set; }
        /// <summary>
        /// 产线编码
        /// </summary>
        public string ProductLineCode { get; set; }
        /// <summary>
        /// 产线名称
        /// </summary>
        public string ProductLineName { get; set; }
        /// <summary>
        /// 拉动条码
        /// </summary>
        public string TaskCode { get; set; }
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
        /// 预计入库时间（yyyy-MM-dd HH:mm:ss）
        /// </summary>
        public string InputEndPlanTime { get; set; }
        /// <summary>
        /// 预计转运时间（yyyy-MM-dd HH:mm:ss）
        /// </summary>
        public string TransEndPlanTime { get; set; }
        /// <summary>
        /// 预计投料时间（yyyy-MM-dd HH:mm:ss）
        /// </summary>
        public string DeliveryEndPlanTime { get; set; }
        /// <summary>
        /// 发送时间（yyyy-MM-dd HH:mm:ss）
        /// </summary>
        public string TransTime { get; set; }
        /// <summary>
        /// 投料方式
        /// </summary>
        //public string FeedingMode { get; set; }
        /// <summary>
        /// 波次
        /// </summary>
        public string WaveNum { get; set; }
        /// <summary>
        /// 批次
        /// </summary>
        public string BatchNum { get; set; }
        /// <summary>
        /// 包装顺序
        /// </summary>
        public string PackSort { get; set; }
        /// <summary>
        /// 投料点编码
        /// </summary>
        public string FeedingPointCode { get; set; }
        /// <summary>
        /// 投料点名称
        /// </summary>
        public string FeedingPointName { get; set; }
        /// <summary>
        /// 装车组名称
        /// </summary>
        public string CarGroupName { get; set; }
        /// <summary>
        /// 装车组编码
        /// </summary>
        public string CarGroupCode { get; set; }
    }
}
