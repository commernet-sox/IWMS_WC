using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.Core;
using iWMS.WebApplication;

namespace iWMS.WC2API.Models
{
    public class MESStationRequest:IEvent<Outcome>
    {
        /// <summary>
        /// 所属产线编码
        /// </summary>
        public string ProductlineCode { get; set; }
        /// <summary>
        /// 工位编码
        /// </summary>
        public string StationCode { get; set; }
        /// <summary>
        /// 工位名称
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// 工段
        /// </summary>
        public string StationRange { get; set; }
        /// <summary>
        /// 工位间隔时间
        /// </summary>
        public int? IntervalTime { get; set; }
        /// <summary>
        /// 过点拉动工位（多个工位编码，以逗号间隔）
        /// </summary>
        public string PassPullStationCode { get; set; }
        /// <summary>
        /// 过点拉动提前时间
        /// </summary>
        public int? PassPullAdvanceTime { get; set; }
        /// <summary>
        /// 顺序号
        /// </summary>
        public int? SequenceNum { get; set; }
    }
}