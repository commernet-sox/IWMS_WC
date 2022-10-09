using Common.ISource;
using FWCore;
using iWMS.BlueSkyAPI.Models;
using iWMS.Core;
using iWMS.TypedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.WebApplication;

namespace iWMS.BlueSkyAPI.Handlers
{
	/// <summary>
	/// 出库任务
	/// </summary>
	public class SOMOrderEventHandler : ISyncEventHandler<SOMOrderRequest, Outcome>
	{
        public Outcome Handle(SOMOrderRequest data)
        {
            if (data == null)
            {
                return new Outcome(400, "数据列表为空");
            }
            if (string.IsNullOrWhiteSpace(data.OrderCode))
            {
                return new Outcome(400, "订单编号不能为空");
            }
            if (string.IsNullOrWhiteSpace(data.StorerId))
            {
                return new Outcome(400, "货主代码不能为空");
            }
            if (string.IsNullOrWhiteSpace(data.OrderType))
            {
                return new Outcome(400, "订单类型不能为空");
            }
            if ((data.Items == null && !data.Items.Any())|| data.Items.Count()==0)
            {
                return new Outcome(400, " 出库通知单明细不能为空");
            }

            string[] skuStatuses = new string[2] { "AVL", "DAMAGE" };

            if (data.Items.Any(t => t.SkuId == null || t.SkuId == ""))
            {
                return new Outcome(400, "物料编码不能为空");
            }
            if (data.Items.Any(t => t.SkuStatus == null || t.SkuStatus == ""))
            {
                return new Outcome(400, "物料状态不能为空");
            }
            if (data.Items.Any(t => !skuStatuses.Contains(t.SkuStatus)))
            {
                return new Outcome(400, "物料状态数据错误");
            }

            var codeData = new T_COM_BaseCodeData();
            var codesql = $"select * from COM_BaseCode where type = 'SOM_OrderType' ";
            var codes = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(codesql, codeData.COM_BaseCode.TableName);
            codeData.Merge(codeData.COM_BaseCode.TableName, codes);
            if (codeData.COM_BaseCode.Count() > 1)
            {
                if (!codeData.COM_BaseCode.Any(x => x.Code == data.OrderType))
                {
                    return new Outcome(400, "订单类型数据错误");
                }
                if (!codeData.COM_BaseCode.Any(x => x.Code == data.BusinessType))
                {
                    return new Outcome(400, "业务类型数据错误");
                }
            }

            string[] orderFlags = new string[2] { "B2C", "B2B" };
            if (!orderFlags.Contains(data.OrderFlag))
            {
                return new Outcome(400, "任务标识数据错误");
            }
            //条码不能重复
            var noDistinct = data.Items.GroupBy(x => x.OrderItemId).All(x => x.Count() == 1);
            if (!noDistinct)
            {
                return new Outcome(400, "订单行号重复");
            }

            var typeData = new T_SOM_OrderData();
            var sql = $"select * from SOM_Order where SyncBillId = '{data.OrderCode}'";
            var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SOM_Order.TableName);
            typeData.Merge(typeData.SOM_Order.TableName, ds);
            //不允许更新，以防止其他程序操作后造成数据异常。
            List<string> error = new List<string>();
            if (typeData.SOM_Order.Count > 0)
            {
                error = typeData.SOM_Order.Select(t => t.SyncBillId).ToList();
                return new Outcome(400, $"{string.Join(",", error)} 出库单已存在，请重新输入！");
            }


            var row = typeData.SOM_Order.NewSOM_OrderRow();
            //获取BillId
            var asn_BillId = GlobalContext.Resolve<ISource_BillHelper>().GetBillId("SO", Commons.StoreId, Commons.WarehouseId);

            HttpContext.Current.SetLogId(asn_BillId);

            row.BillId = asn_BillId;
            row.StorerId = data.StorerId;
            row.SyncBillId = data.OrderCode;
            row.SourceBillId = data.OrderNo;
            row.SOPType = data.OrderFlag;
            row.Status = "12";
            row.WarehouseId = "07";
            row.OrderSource = data.OrderSoure;
            row.OrderType = data.OrderType;
            row.OrderDate = data.OrderDate;
            row.BusinessType = data.BusinessType;
            row.Version = 0;
            row.CreateBy = "System";
            row.CreateDate = GlobalContext.ServerTime;
            row.ShippingCode = data.LogisticsCode;
            row.KdBillId = data.ExpressCode;
            typeData.SOM_Order.AddSOM_OrderRow(row);

            //发货通知单同步表 SOM_SyncOrder
            var syncOrderrow = typeData.SOM_SyncOrder.NewSOM_SyncOrderRow();
            syncOrderrow.BillId = asn_BillId;
            syncOrderrow.SyncBillId = data.OrderCode;
            syncOrderrow.ShopNick = data.ShopNick;
            syncOrderrow.WarehouseId = "07";
            syncOrderrow.StorerId = data.StorerId;
            syncOrderrow.VendorId = data.VendorId;
            syncOrderrow.GoodsCount = data.Items.GroupBy(t => t.SkuId).Count();//SOM_OrderDetail Sku种类
            syncOrderrow.SkuCount = data.Items.Sum(t => t.Qty).ConvertDecimal();//SOM_OrderDetail Sku数量
            syncOrderrow.Memo = data.Memo;
            syncOrderrow.ReceiverCompany = data.ReceiverCompany;
            syncOrderrow.ReceiverName = data.ReceiverName;
            syncOrderrow.ReceiverCountry = data.ReceiverCountry;
            syncOrderrow.ReceiverProvince = data.ReceiverProvince;
            syncOrderrow.ReceiverCity = data.ReceiverCity;
            syncOrderrow.ReceiverArea = data.ReceiverArea;
            syncOrderrow.ReceiverTown = data.ReceiverTown;
            syncOrderrow.ReceiverAddress = data.ReceiverAddress;
            syncOrderrow.ReceiverZip = data.ReceiverZip;
            syncOrderrow.ReceiverPhone = data.ReceiverPhone;
            syncOrderrow.ReceiverMobile = data.ReceiverMobile;
            syncOrderrow.ReceiverEmail = data.ReceiverEmail;
            syncOrderrow.UserName = data.BuyerNick;
            syncOrderrow.PayNo = data.PayNo;
            syncOrderrow.PayName = data.PayName;
            syncOrderrow.AddTime = data.CreateTime;
            syncOrderrow.PayTime = data.PayTime;
            syncOrderrow.BuyMsg = data.BuyerMessage;
            syncOrderrow.TotalAmount = data.TotalAmount;
            syncOrderrow.OrderTotalAmount = data.ItemAmount;
            syncOrderrow.ShipingFee = data.Freight;
            syncOrderrow.CodFee = data.ServiceFee;
            syncOrderrow.Payment = data.ArAmount;
            syncOrderrow.OrderAmount = data.GotAmount;
            syncOrderrow.InvoiceType = data.InvoiceType;
            syncOrderrow.InvoiceContent = data.InvoiceContent;
            syncOrderrow.InvoicePay = data.TaxNumber;
            syncOrderrow.InvoiceTitle = data.InvoiceTitle;
            syncOrderrow.InvoiceAmount = data.InvoiceAmount;
            syncOrderrow.IsCod = data.IsCod;
            syncOrderrow.ShippingTime = data.DeliveryDate;
            syncOrderrow.Priority = data.Priority;
            syncOrderrow.SenderCompany = data.SenderCompany;
            syncOrderrow.SenderCountry = data.SenderCountry;
            syncOrderrow.SenderName = data.SenderName;
            syncOrderrow.SenderProvince = data.SenderProvince;
            syncOrderrow.SenderCity = data.SenderCity;
            syncOrderrow.SenderArea = data.SenderArea;
            syncOrderrow.SenderCountry = data.SenderCountry;
            syncOrderrow.SenderTown = data.SenderTown;
            syncOrderrow.SenderAddress = data.SenderAddress;
            syncOrderrow.SenderZip = data.SenderZip;
            syncOrderrow.SenderPhone = data.SenderPhone;
            syncOrderrow.SenderMobile = data.SenderMobile;
            syncOrderrow.SenderEmail = data.SenderEmail;
            typeData.SOM_SyncOrder.AddSOM_SyncOrderRow(syncOrderrow);

            foreach (var item in data.Items)
            {
                var rowDtl = typeData.SOM_OrderDetail.NewSOM_OrderDetailRow();
                rowDtl.StorerId = row.StorerId;
                rowDtl.OrderLineNo = item.OrderItemId;
                rowDtl.BillId = row.BillId;
                rowDtl.GoodsSn = item.GoodsSN;
                rowDtl.GoodsAtrribute = item.GoodsAttribute;
                rowDtl.GoodsDesc = item.GoodsDesc;
                rowDtl.SkuId = item.SkuId;
                rowDtl.SkuStatus = item.SkuStatus ?? "AVL";
                rowDtl.Price = item.Price;
                rowDtl.Qty = item.Qty;
                rowDtl.Status = "12";
                rowDtl.BatchNo = item.BatchNo;
                rowDtl.ProductDate = item.ProductDate;
                rowDtl.ExpiryDate = item.ExpiryDate;
                rowDtl.PackageCode = "PCS";
                rowDtl.PackageQty = 1;
                rowDtl.Version = 0;
                rowDtl.CreateBy = "System";
                rowDtl.CreateDate = GlobalContext.ServerTime;
                rowDtl.Memo = item.Memo;
                typeData.SOM_OrderDetail.AddSOM_OrderDetailRow(rowDtl);
            }

            GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
            typeData.AcceptChanges();
            return new Outcome();
        }
    }

}