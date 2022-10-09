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
    /// 入库任务
    /// </summary>
    public class SRMOrderEventHandler : ISyncEventHandler<SRMOrderRequest, Outcome>
    {
        public Outcome Handle(SRMOrderRequest data)
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
            if (string.IsNullOrWhiteSpace(data.BusinessType))
            {
                return new Outcome(400, "业务类型不能为空");
            }
            if (string.IsNullOrWhiteSpace(data.OrderType))
            {
                return new Outcome(400, "订单类型不能为空");
            }

            var codeData = new T_COM_BaseCodeData();
            var codesql = $"select * from COM_BaseCode where type = 'SRM_OrderType' ";
            var codes = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(codesql, codeData.COM_BaseCode.TableName);
            codeData.Merge(codeData.COM_BaseCode.TableName, codes);
            if (codeData.COM_BaseCode.Count() > 1) 
            {
                if (!codeData.COM_BaseCode.Any(x=>x.Code== data.OrderType))
                {
                    return new Outcome(400, "订单类型数据错误");
                }
                if (!codeData.COM_BaseCode.Any(x => x.Code == data.BusinessType))
                {
                    return new Outcome(400, "业务类型数据错误");
                }
            }
            if ((data.Items == null && !data.Items.Any())|| data.Items.Count() == 0)
            {
                return new Outcome(400, "入库通知单明细不能为空");
            }

            if (data.Items.Any(t => t.SkuId == null || t.SkuId == ""))
            {
                return new Outcome(400, "物料编码不能为空");
            }
            if (data.Items.Any(t => t.SkuStatus == null || t.SkuStatus == ""))
            {
                return new Outcome(400, "物料状态不能为空");
            }
            //if (data.Items.Any(t => t.BatchNo == null || t.BatchNo == ""))
            //{
            //    return new Outcome(400, "批次编码不能为空");
            //}

            var cidData = new T_COM_ContainerData();
            if (!String.IsNullOrWhiteSpace(data.Cid))
            {
                var cidsql = $"select * from COM_Container where CID = '{data.Cid}' ";
                var cids = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(cidsql, cidData.COM_Container.TableName);
                cidData.Merge(cidData.COM_Container.TableName, cids);
                if (cidData.COM_Container.Count() < 1)
                {
                    return new Outcome(400, "该物料容器不在本地容器数据中");
                }
                else if(data.OrderType == "big" && cidData.COM_Container.FirstOrDefault().CType != "03") 
                {
                    return new Outcome(400, "大件物料的容器类型需要为03");
                }
            }
            //条码不能重复
            var noDistinct = data.Items.GroupBy(x => x.OrderItemId).All(x => x.Count() == 1);
            if (!noDistinct)
            {
                return new Outcome(400, "订单行号重复");
            }

            //入库单需要在物料信息内 
            var skuData = new T_COM_SKUData();
            foreach (var skuids in data.Items)
            {
                var skusql = $"select * from COM_SKU where skuid = '{skuids.SkuId}' and StorerId = 'BSE'";
                var skuds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(skusql, skuData.COM_SKU.TableName);
                skuData.Merge(skuData.COM_SKU.TableName, skuds);
                if (skuData.COM_SKU.Count() < 1)
                {
                    return new Outcome(400, "待入库物料不在本地物料信息中");
                }
            }

            var typeData = new T_SRM_OrderData();
            var sql = $"select * from SRM_Order where SyncBillId = '{data.OrderCode}'";
            var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SRM_Order.TableName);
            typeData.Merge(typeData.SRM_Order.TableName, ds);

            //不允许更新，以防止其他程序操作后造成数据异常。
            List<string> error = new List<string>();
            if (typeData.SRM_Order.Count > 0)
            {
                error = typeData.SRM_Order.Select(t => t.SyncBillId).ToList();
                return new Outcome(400, $"{string.Join(",", error)} 入库单已存在，请重新输入！");
            }

            var row = typeData.SRM_Order.NewSRM_OrderRow();
            //获取BillId  
            var asn_BillId = GlobalContext.Resolve<ISource_BillHelper>().GetBillId("ASN", Commons.StoreId, Commons.WarehouseId);
            HttpContext.Current.SetLogId(asn_BillId);

            row.BillId = asn_BillId;
            row.StorerId = data.StorerId;
            row.WarehouseId = "07";
            row.InOutType = 0;
            row.OrderType = data.OrderType ?? "NORMAL";
            row.SyncBillId = data.OrderCode;
            row.SyncBillDate = DateTime.Now;
            row.OrigBillType = 1;
            row.OrigBillId = data.OrderNo;
            row.BusinessType = data.BusinessType ?? "NORMAL";
            row.VendorId = data.VendorId;
            row.CustomerId = data.CustomerId;
            row.OrderDate = data.OrderDate;
            row.Status = "10";
            row.ReceiverName = data.ReceiverName;
            row.ReceiverCountry = data.ReceiverCountry;
            row.ReceiverProvince = data.ReceiverProvince;
            row.ReceiverCity = data.ReceiverCity;
            row.ReceiverArea = data.ReceiverArea;
            row.ReceiverTown = data.ReceiverTown;
            row.ReceiverAddress = data.ReceiverAddress;
            row.ReceiverZip = data.ReceiverZip;
            row.ReceiverPhone = data.ReceiverPhone;
            row.ReceiverMobile = data.ReceiverMobile;
            row.ReceiverEmail = data.ReceiverEmail;
            row.DealCode = data.DealCode;
            row.ShopCode = data.ShopCode;
            row.ReturnReason = data.ReturnReason;
            row.KdBillId = data.ExpressCode;
            row.CarNo = data.CarNo;
            row.IsCrossDocking = data.IsCrossDocking;
            row.GoodsCount = data.Items.GroupBy(t => t.SkuId).Count();//SRM_OrderDetail Sku种类和
            row.SkuCount = data.Items.Sum(t => t.Qty).ConvertDecimal();//SRM_OrderDetail Sku数量和
            row.IsCharge = "0";
            row.CID = data.Cid;
            row.BatchNo = data.Items.FirstOrDefault().BatchNo;
            row.UDF01 = data.VendorId;
            row.UDF02 = data.VendorName;
            row.UDF03 = data.CustomerId;
            row.UDF04 = data.CustomerName;
            row.Memo = data.Memo;
            row.Version = 0;
            row.CreateBy = "System";
            row.CreateDate = GlobalContext.ServerTime;
            typeData.SRM_Order.AddSRM_OrderRow(row);

            foreach (var item in data.Items)
            {
                var rowDtl = typeData.SRM_OrderDetail.NewSRM_OrderDetailRow();
                rowDtl.OrderLineNo = item.OrderItemId;
                rowDtl.BillId = row.BillId;
                rowDtl.StorerId = data.StorerId;
                rowDtl.SkuId = item.SkuId;
                rowDtl.SkuStatus = "AVL";
                rowDtl.QtyPlan = item.Qty;
                rowDtl.BatchNo = item.BatchNo;
                rowDtl.ProductDate = item.ProductDate;
                rowDtl.ExpiryDate = item.ExpiryDate;
                rowDtl.PackageCode = "PCS";
                rowDtl.PackageQty = 1;
                rowDtl.Version = 1;
                rowDtl.CreateBy = "System";
                rowDtl.CreateDate = GlobalContext.ServerTime;
                typeData.SRM_OrderDetail.AddSRM_OrderDetailRow(rowDtl);
            }
            GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
            typeData.AcceptChanges();
            return new Outcome();
        }
    }
}