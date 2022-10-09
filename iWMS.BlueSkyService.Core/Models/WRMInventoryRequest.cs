using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.BlueSkyService.Core.Models
{
    public class WRMInventoryRequest 
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
        /// WMS盘点单号
        /// </summary>
        public string WmsOrderId { get; set; }
        /// <summary>
        /// 盘点单完成时间
        /// </summary>
        public DateTime OperateTime { get; set; }
        /// <summary>
        /// 订单日期
        /// </summary>
        public DateTime OrderDate { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 盘点单明细
        /// </summary>
        public List<WRMInventoryDetail> Items { get; set; }
    }
    public class WRMInventoryDetail
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
        /// 容器号
        /// </summary>
        public string Cid { get; set; }
        /// <summary>
        /// 盘点下发数量
        /// </summary>
        public decimal QtyPlan { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }
        /// <summary>
        /// 批次编码
        /// </summary>
        public string BatchNo { get; set; }
    }
}
