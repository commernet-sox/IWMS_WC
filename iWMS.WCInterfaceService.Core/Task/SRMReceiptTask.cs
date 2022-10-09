using Common.ISource;
using FWCore;
using FWCore.TaskManage;
using iWMS.TypedData;
using iWMS.WCInterfaceService.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCInterfaceService.Core.Task
{
    /// <summary>
    /// 入库任务回传
    /// </summary>
    public class SRMReceiptTask: CommonBaseTask
    {
        protected override bool Execute(TaskConfig config)
        {
            var typeData = new T_SRM_ReceiptData();
            var orderStatusData = new T_SYS_OrderSyncStatusData();
            // 55接受14拒收
            var sql = $"SELECT TOP 10 * FROM SRM_Order A WHERE A.UDF01 IN('3','4') AND NOT EXISTS(SELECT 1 FROM SYS_Order_Sync_Status WHERE OrderId = A.BillId)";
            var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SRM_Receipt.TableName);
            //var ds = BusinessDbUtil.GetDataSet(sql, typeData.MES_PointTask.TableName);
            typeData.Merge(typeData.SRM_Receipt.TableName, ds);
            var sqlDtl = $"select a.BillId,a.UDF02,a.UDF03,a.SyncBillId,a.CID,a.OrderDate,a.UDF01,a.ModifyBy,a.ModifyDate,b.SkuId,b.Memo,g.SkuName,(select d.QtyIn from SRM_Scan c inner join SRM_ScanDetail d on c.BillId = d.BillId where c.OrigBillId = a.BillId and d.OrderLineNo = b.OrderLineNo and d.SkuId = d.SkuId) QtyIn,b.UDF08,b.OrderLineNo from SRM_Order a inner join SRM_OrderDetail b on a.BillId = b.BillId inner join COM_SKU g on b.SkuId = g.SkuId where a.BillId in ('{string.Join("','", typeData.SRM_Receipt.Select(t => t.BillId))}') and not exists(select 1 from SRM_OrderDetail where BillId = a.BillId and OrderLineNo = b.OrderLineNo and UDF20 not in ('3', '4'))";

            var dsDtl = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sqlDtl);
            var dt = dsDtl.Tables[0];

            if (typeData.SRM_Receipt.Count > 0 && dt.Rows.Count > 0)
            {
                foreach (var rItem in typeData.SRM_Receipt)
                {
                    var rdList = dt.AsEnumerable().Where(t => t["BillId"].ConvertString() == rItem.BillId).ToList();
                    //允许传多个
                    List<SRMReceiptRequest> requests = new List<SRMReceiptRequest>();
                    foreach (var rdItem in rdList)
                    {
                        SRMReceiptRequest request = new SRMReceiptRequest();
                        request.ProductLineCode = rdItem["UDF02"].ConvertString();
                        request.ProductLineName = rdItem["UDF03"].ConvertString();
                        request.InputSerial = rdItem["SyncBillId"].ConvertString();
                        request.Appliance = rdItem["CID"].ConvertString();
                        request.PlanDate = rdItem["OrderDate"].ConvertDateTime() == null ? DateTime.Now.ToString("yyyy-MM-dd") : rdItem["OrderDate"].ConvertDateTime().Value.ToString("yyyy-MM-dd");
                        request.PullNumber = rdItem["UDF08"].ConvertString();
                        request.Status = rdItem["UDF01"].ConvertString();
                        request.NoMatchReason = rdItem["Memo"].ConvertString();
                        request.AcceptName = rdItem["ModifyBy"].ConvertString();
                        request.AcceptTime = rdItem["ModifyDate"].ConvertDateTime() ?? DateTime.Now;
                        request.MaterialCode = rdItem["SkuId"].ConvertString();
                        request.MaterialName = rdItem["SkuName"].ConvertString();
                        request.Quantity = rdItem["QtyIn"].ConvertLong();
                        request.WarehouseCode = "5221ZCL";
                        request.WarehouseName = "2号工厂_总装1车间材料库";
                        request.AgvWarehouseCode = "QZH";
                        request.AgvWarehouseName = "二号厂QZH缓存区";
                        //request.InputSerialDetail = rdItem["OrderLineNo"].ConvertString();
                        request.DetailId = rdItem["OrderLineNo"].ConvertString();
                        requests.Add(request);
                    }

                    var oc = ApiFactory.Create("SC/LES/SaveInputTaskToGMSComeback").WithBody(requests).LogResult();
                    Console.WriteLine(DateTime.Now);
                    //回传成功后更新主表信息
                    if (oc.IsSuccess)
                    {
                        var row = orderStatusData.SYS_Order_Sync_Status.NewSYS_Order_Sync_StatusRow();
                        row.OrderId = rItem.BillId;
                        row.Status = "10";
                        row.OPT_By = "";
                        row.OPT_Date = DateTime.Now;
                        row.CreateBy = "WC2API";
                        row.CreateDate = DateTime.Now;
                        row.Memo = "";
                        orderStatusData.SYS_Order_Sync_Status.AddSYS_Order_Sync_StatusRow(row);
                        GlobalContext.Resolve<ISource_SQLHelper>().Save(orderStatusData);
                        typeData.AcceptChanges();
                    }
                }
            }

            return true;


            //临时回传
            //var typeData = new T_SRM_OrderData();
            //var orderStatusData = new T_SYS_OrderSyncStatusData();
            //var sql = $"SELECT TOP 10 * FROM SRM_Order A WHERE A.Status IN('10','0') AND NOT EXISTS(SELECT 1 FROM SYS_Order_Sync_Status WHERE OrderId = A.BillId)";
            //var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SRM_Order.TableName);
            //typeData.Merge(typeData.SRM_Order.TableName, ds);

            //var dtlsql = $"SELECT * FROM SRM_OrderDetail A WHERE A.BillId IN('{string.Join("','", typeData.SRM_Order.Select(t => t.BillId))}') ";
            //var dtlds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(dtlsql, typeData.SRM_OrderDetail.TableName);
            //typeData.Merge(typeData.SRM_OrderDetail.TableName, dtlds);

            //if (typeData.SRM_Order.Count > 0)
            //{
            //    foreach (var rItem in typeData.SRM_Order)
            //    {
            //        var srmDtl = typeData.SRM_OrderDetail.Where(t => t.BillId == rItem.BillId).ToList();
            //        //允许传多个
            //        List<SRMReceiptRequest> requests = new List<SRMReceiptRequest>();

            //        foreach (var item in srmDtl)
            //        {
            //            SRMReceiptRequest request = new SRMReceiptRequest();
            //            request.ProductLineCode = item.UDF02;
            //            request.ProductLineName = item.UDF03;
            //            request.InputSerial = rItem.SyncBillId;
            //            request.Appliance = rItem.CID;
            //            request.PlanDate = rItem.CreateDate.ToString("yyyy-MM-dd");
            //            request.PullNumber = item.UDF08;
            //            request.Status = rItem.Status=="10"?"3":"4";
            //            request.NoMatchReason = "";
            //            request.AcceptName = rItem.CreateBy;
            //            request.AcceptTime = rItem.CreateDate;
            //            request.MaterialCode = item.SkuId;
            //            request.MaterialName = item.UDF05;
            //            request.Quantity = item.QtyIn.ConvertLong();
            //            request.WarehouseCode = "5221ZCL";
            //            request.WarehouseName = "2号工厂_总装1车间材料库";
            //            request.AgvWarehouseCode = "QZH";
            //            request.AgvWarehouseName = "二号厂QZH缓存区";
            //            //request.InputSerialDetail = rdItem["OrderLineNo"].ConvertString();
            //            request.DetailId = item.OrderLineNo;
            //            requests.Add(request);
            //        }

            //        var oc = ApiFactory.Create("SC/LES/SaveInputTaskToGMSComeback").WithBody(requests).LogResult();
            //        Console.WriteLine(DateTime.Now);
            //        //回传成功后更新主表信息
            //        if (oc.IsSuccess)
            //        {
            //            var row = orderStatusData.SYS_Order_Sync_Status.NewSYS_Order_Sync_StatusRow();
            //            row.OrderId = rItem.BillId;
            //            row.Status = "10";
            //            row.OPT_By = "";
            //            row.OPT_Date = DateTime.Now;
            //            row.CreateBy = "WC2API";
            //            row.CreateDate = DateTime.Now;
            //            row.Memo = "";
            //            orderStatusData.SYS_Order_Sync_Status.AddSYS_Order_Sync_StatusRow(row);
            //            GlobalContext.Resolve<ISource_SQLHelper>().Save(orderStatusData);
            //            typeData.AcceptChanges();
            //        }
            //    }
            //}

            //return true;

        }
    }
}
