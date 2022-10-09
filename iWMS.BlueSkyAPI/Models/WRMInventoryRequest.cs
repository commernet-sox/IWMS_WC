using iWMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.WebApplication;

namespace iWMS.BlueSkyAPI.Models
{
    public class WRMInventoryRequest : IEvent<Outcome>
    {
        /// <summary>
        /// 盘点单编号（唯一值）
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 货主代码
        /// </summary>
        public string StorerId { get; set; }
        /// <summary>
        /// 盘点方式
        /// </summary>
        public string InventoryMode { get; set; }
        /// <summary>
        /// 盘点方式
        /// </summary>
        public string InventoryType { get; set; }
        /// <summary>
        /// 盘点开始时间
        /// </summary>
        public DateTime BeginDate { get; set; }
        /// <summary>
        /// 盘点结束时间
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// 订单日期
        /// </summary>
        public DateTime OrderDate { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 盘点单明细
        /// </summary>
        public List<WRM_InventoryRangeDetail> Items { get; set; }
    }
    public class WRM_InventoryRangeDetail
    {
        /// <summary>
        /// 订单行号
        /// </summary>
        public string OrderItemId { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string SkuId { get; set; }
        /// <summary>
        /// 物料状态
        /// </summary>
        public string SkuStatus { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }
    }
}