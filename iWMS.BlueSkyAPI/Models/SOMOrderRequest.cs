using iWMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.WebApplication;

namespace iWMS.BlueSkyAPI.Models
{
    public class SOMOrderRequest : IEvent<Outcome>
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
        /// 任务标识
        /// </summary>
        public string OrderFlag { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusinessType { get; set; }
        /// <summary>
        /// 关联单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 订单日期
        /// </summary>
        public DateTime OrderDate { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string OrderSoure { get; set; }
        /// <summary>
        /// 下单时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime PayTime { get; set; }
        /// <summary>
        /// 支付平台交易号
        /// </summary>
        public string PayNo { get; set; }
        /// <summary>
        /// 支付工具
        /// </summary>
        public string PayName { get; set; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopNick { get; set; }
        /// <summary>
        /// 买家名称
        /// </summary>
        public string SellerNick { get; set; }
        /// <summary>
        /// 买家名称
        /// </summary>
        public string BuyerNick { get; set; }
        /// <summary>
        /// 订单总金额 (元)
        /// </summary>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// 商品总金额 (元) 
        /// </summary>
        public decimal ItemAmount { get; set; }
        /// <summary>
        /// 快递费用 (元)
        /// </summary>
        public decimal Freight { get; set; }
        /// <summary>
        /// 应收金额 (元)  
        /// </summary>
        public decimal ArAmount { get; set; }
        /// <summary>
        /// 已收金额 (元) 
        /// </summary>
        public decimal GotAmount { get; set; }
        /// <summary>
        /// COD 服务费
        /// </summary>
        public decimal ServiceFee { get; set; }
        /// <summary>
        /// 货到付款
        /// </summary>
        public int IsCod { get; set; }
        /// <summary>
        /// 物流公司编码
        /// </summary>
        public string LogisticsCode { get; set; }
        /// <summary>
        /// 运单号
        /// </summary>
        public string ExpressCode { get; set; }
        /// <summary>
        /// 寄件人公司
        /// </summary>
        public string SenderCompany { get; set; }
        /// <summary>
        /// 寄件人姓名
        /// </summary>
        public string SenderName { get; set; }
        /// <summary>
        /// 寄件人国家
        /// </summary>
        public string SenderCountry { get; set; }
        /// <summary>
        /// 寄件人省
        /// </summary>
        public string SenderProvince { get; set; }
        /// <summary>
        /// 寄件人市
        /// </summary>
        public string SenderCity { get; set; }
        /// <summary>
        /// 寄件人区
        /// </summary>
        public string SenderArea { get; set; }
        /// <summary>
        /// 寄件人镇/街道
        /// </summary>
        public string SenderTown { get; set; }
        /// <summary>
        /// 寄件人地址
        /// </summary>
        public string SenderAddress { get; set; }
        /// <summary>
        /// 寄件人邮编
        /// </summary>
        public string SenderZip { get; set; }
        /// <summary>
        /// 寄件人电话
        /// </summary>
        public string SenderPhone { get; set; }
        /// <summary>
        /// 寄件人手机
        /// </summary>
        public string SenderMobile { get; set; }
        /// <summary>
        /// 寄件人邮箱
        /// </summary>
        public string SenderEmail { get; set; }
        /// <summary>
        /// 收件人公司
        /// </summary>
        public string ReceiverCompany { get; set; }
        /// <summary>
        /// 收件人姓名
        /// </summary>
        public string ReceiverName { get; set; }
        /// <summary>
        /// 收件人国家
        /// </summary>
        public string ReceiverCountry { get; set; }
        /// <summary>
        /// 收件人省
        /// </summary>
        public string ReceiverProvince { get; set; }
        /// <summary>
        /// 收件人市
        /// </summary>
        public string ReceiverCity { get; set; }
        /// <summary>
        /// 收件人区
        /// </summary>
        public string ReceiverArea { get; set; }
        /// <summary>
        /// 收件人镇/街道
        /// </summary>
        public string ReceiverTown { get; set; }
        /// <summary>
        /// 收件人地址
        /// </summary>
        public string ReceiverAddress { get; set; }
        /// <summary>
        /// 收件人邮编
        /// </summary>
        public string ReceiverZip { get; set; }
        /// <summary>
        /// 收件人电话
        /// </summary>
        public string ReceiverPhone { get; set; }
        /// <summary>
        /// 收件人手机
        /// </summary>
        public string ReceiverMobile { get; set; }
        /// <summary>
        /// 收件人邮箱
        /// </summary>
        public string ReceiverEmail { get; set; }
        /// <summary>
        /// 发票类型
        /// </summary>
        public string InvoiceType { get; set; }
        /// <summary>
        /// 发票抬头
        /// </summary>
        public string InvoiceContent { get; set; }
        /// <summary>
        /// 税号
        /// </summary>
        public string TaxNumber { get; set; }
        /// <summary>
        /// 发票内容
        /// </summary>
        public string InvoiceTitle { get; set; }
        /// <summary>
        /// 发票总金额
        /// </summary>
        public string InvoiceAmount { get; set; }
        /// <summary>
        /// 发票商品信息
        /// </summary>
        public string InvoiceItems { get; set; }
        /// <summary>
        /// 买家留言
        /// </summary>
        public string BuyerMessage { get; set; }
        /// <summary>
        /// 卖家留言
        /// </summary>
        public int SellerMessage { get; set; }
        /// <summary>
        /// 配送时间
        /// </summary>
        public DateTime DeliveryDate { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string VendorId { get; set; }
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string VendorName { get; set; }
        /// <summary>
        /// 扩展数据
        /// </summary>
        public string ExtendProps { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 出库单明细
        /// </summary>
        public List<SOM_OrderDetail> Items { get; set; }
    }
    public class SOM_OrderDetail
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
        /// <summary>
        /// 价格(元)
        /// </summary>
        public decimal Price { get; set; }
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
        /// <summary>
        /// 商品货号
        /// </summary>
        public string GoodsSN { get; set; }
        /// <summary>
        /// 商品属性
        /// </summary>
        public string GoodsAttribute { get; set; }
        /// <summary>
        /// 商品描述
        /// </summary>
        public string GoodsDesc { get; set; }
        /// <summary>
        /// 平台交易号
        /// </summary>
        public string DealCode { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Memo { get; set; }
    }
}