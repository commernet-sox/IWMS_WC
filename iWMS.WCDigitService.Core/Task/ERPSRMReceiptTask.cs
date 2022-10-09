using Common.ISource;
using FWCore;
using FWCore.TaskManage;
using iWMS.TypedData;
using iWMS.WCDigitService.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCDigitService.Core.Task
{
    /// <summary>
    /// 物料入库反馈（直投件）
    /// </summary>
    public class ERPSRMReceiptTask : CommonBaseTask
    {
        protected override bool Execute(TaskConfig config)
        {
            var typeData = new T_SRM_ReceiptData();
            var orderStatusData = new T_SYS_OrderSyncStatusData();
            var sql = $"SELECT TOP 10 * FROM SRM_Receipt A WHERE A.Status IN('55') AND NOT EXISTS(SELECT 1 FROM SYS_Order_Sync_Status WHERE OrderId = A.BillId)";
            var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SRM_Receipt.TableName);
            //var ds = BusinessDbUtil.GetDataSet(sql, typeData.MES_PointTask.TableName);
            typeData.Merge(typeData.SRM_Receipt.TableName, ds);

            var sqlDtl = $"select * SRM_Receipt d INNER JOIN SRM_ReceiptDetail h ON h.BillId=d.BillId where d.BillId in ({string.Join("','", typeData.SRM_Receipt.Select(t => t.BillId))})";
            var dsDtl = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sqlDtl);
            var dt = dsDtl.Tables[0];

            if (typeData.SRM_Receipt.Count > 0 && dt.Rows.Count > 0)
            {
                foreach (var rItem in typeData.SRM_Receipt)
                {
                    var rdtl = dt.AsEnumerable().Where(t => t["BillId"].ConvertString() == rItem.BillId).FirstOrDefault();
                    //允许传多个

                    ERPSRMReceiptRequest request = new ERPSRMReceiptRequest();
                    request.LineId = rdtl["FlowNo"].ConvertLong();
                    request.OrgId = rItem.UDF01;
                    request.OrganizationId = rdtl["UDF01"].ConvertString();
                    request.ShipmentHeaderId = rItem.UDF02;
                    request.ShipmentNum = rItem.SyncBillId;
                    request.GmsNumber = rItem.BillId;
                    request.PoNumber = rItem.UDF07;
                    request.PoHeaderId = rItem.UDF05;
                    request.PoReleaseId = rItem.UDF06;
                    request.ReleaseNumber = rItem.UDF08;
                    request.PoLineId = rdtl["UDF07"].ConvertString();
                    request.PoLineNumber = rdtl["UDF08"].ConvertString();
                    request.VendorsCode = rItem.UDF03;
                    request.VendorsName = rItem.UDF04;
                    request.ItemId = rdtl["UDF09"].ConvertString();
                    request.ItemCode = rdtl["SkuId"].ConvertString();
                    request.ItemDes = rdtl["UDF10"].ConvertString();
                    request.Unit = rdtl["UDF11"].ConvertString();
                    request.LotCode = rdtl["BatchNo"].ConvertString();
                    request.ReceiptQuantity = rdtl["QtyIn"].ConvertString();
                    request.ReceiptDate = rItem.CreateDate;
                    //request.LocatorName = "";
                    request.LastUpdateTime = rItem.ModifyDate;

                    var oc = ApiFactory.Create("xx").WithBody(request).LogResult();
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
                        GlobalContext.Resolve<ISource_SQLHelper>().Save(orderStatusData);
                    }
                }
            }

            return true;
        }
    }
}
