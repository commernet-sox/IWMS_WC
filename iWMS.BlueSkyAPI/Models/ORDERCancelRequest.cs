using iWMS.Core;
using iWMS.WebApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iWMS.BlueSkyAPI.Models
{
    public class ORDERCancelRequest : IEvent<Outcome>
    {
        /// <summary>
        /// 任务编号(唯一)
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 货主代码
        /// </summary>
        public string OrderType { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string StorerId { get; set; }
        /// <summary>
        /// 物料简称
        /// </summary>
        public string Reason { get; set; }
    }
}