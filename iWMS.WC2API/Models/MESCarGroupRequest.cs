using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.Core;
using iWMS.WebApplication;

namespace iWMS.WC2API.Models
{
    public class MESCarGroupRequest : IEvent<Outcome>
    {
        /// <summary>
        /// 装车组名称
        /// </summary>
        public string CarGroupName { get; set; }
        /// <summary>
        /// 装车组编码
        /// </summary>
        public string CarGroupCode { get; set; }
        /// <summary>
        /// 发货区
        /// </summary>
        public string LoadArea { get; set; }
        /// <summary>
        /// 卸货区
        /// </summary>
        public string UnloadArea { get; set; }
        /// <summary>
        /// 汇总波次
        /// </summary>
        public long? BatchNum { get; set; }
        /// <summary>
        /// 投料台数
        /// </summary>
        public long? WaveNum { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 所属产线编码
        /// </summary>
        public string ProductlineCode { get; set; }
    }
}