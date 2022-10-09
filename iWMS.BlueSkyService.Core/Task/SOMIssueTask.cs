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
    /// 出库任务回传
    /// </summary>
    public class SOMIssueTask : CommonBaseTask
    {
        protected override bool Execute(TaskConfig config)
        {
            var typeData = new T_SOM_IssueData();
            var orderStatusData = new T_SYS_OrderSyncStatusData();
            var sql = $"SELECT TOP 10 * FROM SOM_Issue A WHERE A.Status IN('55') AND ISNULL(A.SyncBillId,'')<>'' AND NOT EXISTS(SELECT 1 FROM SYS_Order_Sync_Status WHERE OrderId = A.BillId)";
            var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SOM_Issue.TableName);
            typeData.Merge(typeData.SOM_Issue.TableName, ds);

            foreach (var rDate in typeData.SOM_Issue)
            {
                var sqlDtl = $"select d.BillId,d.OrderDate,d.OrderType,d.SyncBillId,c.SOPType,c.SourceBillId,d.StorerId,d.BusinessType,d.ChargeDate,d.Memo, b.SkuId,b.OrderLineNo,b.SkuStatus,b.QtyScan,b.BatchNo,b.ProductDate,b.ExpiryDate,a.CID from TRM_TaskXJ a inner join TRM_TaskXJDetail b on a.TaskId = b.TaskId inner join SOM_Order c on a.SourceBillId = c.BillId inner join SOM_Issue d on c.BillId = d.OrigBillId where d.BillId = '{rDate.BillId}'";
                var dsDtl = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sqlDtl);
                var dt = dsDtl.Tables[0];

                if (dt.Rows.Count > 0)
                {
                    SOMIssueRequest request = new SOMIssueRequest();
                    request.OrderCode = dt.Rows[0]["SyncBillId"].ConvertString();
                    request.StorerId = dt.Rows[0]["StorerId"].ConvertString(); 
                    request.OrderType = dt.Rows[0]["OrderType"].ConvertString();
                    request.OrderFlag = dt.Rows[0]["SOPType"].ConvertString();
                    request.WmsOrderId = dt.Rows[0]["BillId"].ConvertString();
                    request.BusinessType = dt.Rows[0]["BusinessType"].ConvertString();
                    request.OrderNo = dt.Rows[0]["SourceBillId"].ConvertString();
                    request.OperateTime = dt.Rows[0]["ChargeDate"].ConvertDateTime() ?? DateTime.Now;
                    request.OrderDate = dt.Rows[0]["OrderDate"].ConvertDateTime() ?? DateTime.Now;
                    request.Memo = dt.Rows[0]["Memo"].ConvertString();
                    request.Items = new List<SOMIssueDetail>();
                    foreach (DataRow rdItem in dt.Rows)
                    {
                        var productDate = rdItem["ProductDate"].ConvertDateTime();
                        var expiryDate = rdItem["ExpiryDate"].ConvertDateTime();
                        SOMIssueDetail requestDetail = new SOMIssueDetail();
                        requestDetail.OrderItemId = rdItem["OrderLineNo"].ConvertString();
                        requestDetail.SkuId = rdItem["SkuId"].ConvertString();
                        requestDetail.SkuStatus = rdItem["SkuStatus"].ConvertString();
                        requestDetail.Cid = rdItem["CID"].ConvertString();
                        requestDetail.Qty = rdItem["QtyScan"].ConvertInt32();
                        requestDetail.BatchNo = rdItem["BatchNo"].ConvertString();
                        requestDetail.ProductDate = productDate.HasValue ? productDate.Value.ToString("yyyy-MM-dd") : null;
                        requestDetail.ExpiryDate = expiryDate.HasValue ? expiryDate.Value.ToString("yyyy-MM-dd") : null;
                        request.Items.Add(requestDetail);
                    }
                    var oc = ApiFactory.Create("/gz_wms_delivery_confirm").WithBody(request).LogResult();
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
