using iWMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.WebApplication;

namespace iWMS.WC2API.Models
{
    /// <summary>
    /// 产线MES投料任务下发
    /// </summary>
    public class ProMESPointTaskRequest : IEvent<Outcome>
    {
        /// <summary>
        /// 任务号
        /// </summary>
        public string TaskId { get; set; }
        /// <summary>
        /// 产线编码
        /// </summary>
        public string ProductLineCode { get; set; }
        /// <summary>
        /// 工位编码
        /// </summary>
        public string OperationCode { get; set; }
        /// <summary>
        /// 投料点 (确认过10.8号)
        /// </summary>
        public string LocationId { get; set; }
        /// <summary>
        /// 备用字段1
        /// </summary>
        public string backupStr1 { get; set; }
        /// <summary>
        /// 备用字段2
        /// </summary>
        public string backupStr2 { get; set; }
    }
}