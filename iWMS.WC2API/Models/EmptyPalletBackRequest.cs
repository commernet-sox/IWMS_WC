using iWMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.WebApplication;

namespace iWMS.WC2API.Models
{
    public class EmptyPalletBackRequest:IEvent<Outcome>
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
        /// 取空托位置
        /// </summary>
        public string LocationId { get; set; }
        /// <summary>
        /// 空托盘号
        /// </summary>
        public string PalletId { get; set; }
        /// <summary>
        /// 备用字段1
        /// </summary>
        public string BackupStr1 { get; set; }
        /// <summary>
        /// 备用字段2
        /// </summary>
        public string BackupStr2 { get; set; }
    }
}