using Common.ISource;
using FWCore;
using FWCore.TaskManage;
using iWMS.BlueSkyService.Core.Models;
using iWMS.TypedData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.BlueSkyService.Core.Task
{
    /// <summary>
    /// 盘点单任务回传
    /// </summary>
    public class WRMInventoryTask : CommonBaseTask
    {
        protected override bool Execute(TaskConfig config)
        {
            var typeData = new T_WRM_InventoryData();
            var orderStatusData = new T_SYS_OrderSyncStatusData();
            var sql = $"SELECT TOP 10 * FROM WRM_Inventory A WHERE A.Status IN('55') AND ISNULL(A.OrigBillId,'')<>'' AND NOT EXISTS(SELECT 1 FROM SYS_Order_Sync_Status WHERE OrderId = A.BillId)";
            var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.WRM_Inventory.TableName);
            typeData.Merge(typeData.WRM_Inventory.TableName, ds);

            foreach (var rDate in typeData.WRM_Inventory)
            {
                var sqlDt1 = $"SELECT A.BillId,A.Status,A.OrderDate,A.OrigBillId,A.StorerId,A.ModifyDate,A.Memo,B.FlowNo,B.SkuId,B.SkuStatus,B.CID,B.QtyPlan,B.Qty,B.BatchNo FROM WRM_Inventory A INNER JOIN WRM_InventoryDetail B ON A.BillId = B.BillId WHERE A.BillId = '{rDate.BillId}'";
                var dsDtl = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sqlDt1);
                var dt = dsDtl.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    WRMInventoryRequest request = new WRMInventoryRequest();
                    request.OrderCode = dt.Rows[0]["OrigBillId"].ConvertString(); 
                    request.StorerId = dt.Rows[0]["StorerId"].ConvertString(); 
                    request.WmsOrderId = dt.Rows[0]["BillId"].ConvertString(); 
                    if (rDate.Status=="40")//完成后才传时间
                    {
                        request.OperateTime = dt.Rows[0]["ModifyDate"].ConvertDateTime() ?? DateTime.Now;

                    }
                    request.Memo = dt.Rows[0]["Memo"].ConvertString();
                    request.OrderDate = rDate.OrderDate;
                    request.Items = new List<WRMInventoryDetail>();
                    foreach (DataRow rdItem in dt.Rows)
                    {
                        WRMInventoryDetail requestDetail = new WRMInventoryDetail
                        {
                            OrderItemId = rdItem["FlowNo"].ConvertString(),
                            SkuId = rdItem["SkuId"].ConvertString(),
                            SkuStatus = rdItem["SkuStatus"].ConvertString(),
                            Cid = rdItem["CID"].ConvertString(),
                            Qty = rdItem["Qty"].ConvertInt32(),
                            BatchNo = rdItem["BatchNo"].ConvertString()
                        };
                        request.Items.Add(requestDetail);
                    }

                    var oc = ApiFactory.Create("/gz_wms_inventory_confirm").WithBody(request).LogResult();
                    Console.WriteLine(DateTime.Now);
                    //回传成功后更新主表信息
                    if (oc.IsSuccess)
                    {
                        var row = orderStatusData.SYS_Order_Sync_Status.NewSYS_Order_Sync_StatusRow();
                        row.OrderId = rDate.BillId;
                        row.Status = "10";
                        row.OPT_By = "";
                        row.OPT_Date = DateTime.Now;
                        row.CreateBy = "BlueSkyAPI";
                        row.CreateDate = DateTime.Now;
                        row.Memo = "";
                        orderStatusData.SYS_Order_Sync_Status.AddSYS_Order_Sync_StatusRow(row);
                        GlobalContext.Resolve<ISource_SQLHelper>().Save(orderStatusData);
                    }
                }
            }
            return true;
        }
    }
}


