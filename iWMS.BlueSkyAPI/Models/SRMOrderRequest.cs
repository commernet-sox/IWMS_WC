using iWMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.WebApplication;

namespace iWMS.BlueSkyAPI.Models
{
    public class SRMOrderRequest : IEvent<Outcome>
    {
        /// <summary>
        /// 订单编号（唯一值）
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 货主代码
        /// </summary>
        public string StorerId { get; set; }
        /// <summary>
        /// 订单类型
        /// </summary>
        public string OrderType { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusinessType { get; set; }
        /// <summary>
        /// 关联单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string VendorId { get; set; }
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string VendorName { get; set; }
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CustomerId { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 物流公司编码
        /// </summary>
        public string LogisticsCode { get; set; }
        /// <summary>
        /// 物流公司名称
        /// </summary>
        public string LogisticsName { get; set; }
        /// <summary>
        /// 运单号
        /// </summary>
        public string ExpressCode { get; set; }
        /// <summary>
        /// 店铺
        /// </summary>
        public string ShopCode { get; set; }
        /// <summary>
        /// 交易单号
        /// </summary>
        public string DealCode { get; set; }
        /// <summary>
        /// 收/退件人姓名
        /// </summary>
        public string ReceiverName { get; set; }
        /// <summary>
        /// 收/退件人国家
        /// </summary>
        public string ReceiverCountry { get; set; }
        /// <summary>
        /// 收/退件省
        /// </summary>
        public string ReceiverProvince { get; set; }
        /// <summary>
        /// 收/退件市
        /// </summary>
        public string ReceiverCity { get; set; }
        /// <summary>
        /// 收/退件区
        /// </summary>
        public string ReceiverArea { get; set; }
        /// <summary>
        /// 收/退件镇/街道
        /// </summary>
        public string ReceiverTown { get; set; }
        /// <summary>
        /// 收/退件地址
        /// </summary>
        public string ReceiverAddress { get; set; }
        /// <summary>
        /// 收/退件邮编
        /// </summary>
        public string ReceiverZip { get; set; }
        /// <summary>
        /// 收/退件电话
        /// </summary>
        public string ReceiverPhone { get; set; }
        /// <summary>
        /// 收/退件手机
        /// </summary>
        public string ReceiverMobile { get; set; }
        /// <summary>
        /// 收/退件邮箱
        /// </summary>
        public string ReceiverEmail { get; set; }
        /// <summary>
        /// 退货原因
        /// </summary>
        public string ReturnReason { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public string CarNo { get; set; }
        /// <summary>
        /// 是否越库
        /// </summary>
        public int IsCrossDocking { get; set; }
        /// <summary>
        /// 容器号
        /// </summary>
        public string Cid { get; set; }
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
        /// 入库单明细
        /// </summary>
        public List<SRM_OrderDetail> Items { get; set; }
    }

    public class SRM_OrderDetail
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
        ///    数量
        /// </summary>
        public decimal Qty { get; set; }
        /// <summary>
        /// 批次编码
        /// </summary>
        public string BatchNo { get; set; }
        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime ProductDate { get; set; }
        /// <summary>
        /// 有效日期
        /// </summary>
        public DateTime ExpiryDate { get; set; }
    }
}