using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.BlueSkyService.Core.Models
{
    public class SOMIssueRequest
    {
        /// <summary>
        /// 订单编号(唯一)
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        ///  货主代码
        /// </summary>
        public string StorerId { get; set; }
        /// <summary>
        /// 订单类型
        /// </summary>
        public string OrderType { get; set; }
        /// <summary>
        /// 任务标识
        /// </summary>
        public string OrderFlag { get; set; }
        /// <summary>
        /// WMS入库单号
        /// </summary>
        public string WmsOrderId { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusinessType { get; set; }
        /// <summary>
        /// 关联单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 出库单记账时间
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
        /// 出库单明细
        /// </summary>
        public List<SOMIssueDetail> Items { get; set; }
    }
    public class SOMIssueDetail
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
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }
        /// <summary>
        /// 批次编码
        /// </summary>
        public string BatchNo { get; set; }
        /// <summary>
        /// 生产日期
        /// </summary>
        public string ProductDate { get; set; }
        /// <summary>
        /// 有效日期
        /// </summary>
        public string ExpiryDate { get; set; }
    }
}
