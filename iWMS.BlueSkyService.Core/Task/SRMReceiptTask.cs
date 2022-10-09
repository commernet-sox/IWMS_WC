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
    /// 入库任务回传
    /// </summary>
    public class SRMReceiptTask : CommonBaseTask
    {
        protected override bool Execute(TaskConfig config)
        {
            var typeData = new T_SRM_ReceiptData();
            var orderStatusData = new T_SYS_OrderSyncStatusData();
            var sql = $"SELECT TOP 10 * FROM SRM_Receipt A WHERE A.Status IN('55') AND ISNULL(A.SyncBillId,'')<>'' AND NOT EXISTS(SELECT 1 FROM SYS_Order_Sync_Status WHERE OrderId = A.BillId)";
            var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SRM_Receipt.TableName);
            typeData.Merge(typeData.SRM_Receipt.TableName, ds);

            foreach (var rDate in typeData.SRM_Receipt)
            {
                var sqlDtl = $"select a.BillId,a.OrderDate,a.OrderType,a.SyncBillId,a.OrigBillId,a.StorerId,a.BusinessType,a.ChargeDate,a.Memo, b.SkuId,b.OrderLineNo,b.SkuStatus,b.QtyIn,b.BatchNo,b.ProductDate,b.ExpiryDate from SRM_Receipt a inner join SRM_ReceiptDetail b on a.BillId = b.BillId where a.BillId = '{rDate.BillId}'";
                var dsDtl = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sqlDtl);
                var dt = dsDtl.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    SRMReceiptRequest request = new SRMReceiptRequest();
                    request.OrderCode = dt.Rows[0]["SyncBillId"].ConvertString();
                    request.StorerId = dt.Rows[0]["StorerId"].ConvertString(); 
                    request.OrderType = dt.Rows[0]["OrderType"].ConvertString(); 
                    request.WmsOrderId = dt.Rows[0]["BillId"].ConvertString();
                    request.BusinessType = dt.Rows[0]["BusinessType"].ConvertString(); 
                    request.OrderNo = dt.Rows[0]["OrigBillId"].ConvertString();
                    request.OperateTime = dt.Rows[0]["ChargeDate"].ConvertDateTime() ?? DateTime.Now;
                    request.Memo = dt.Rows[0]["Memo"].ConvertString();
                    request.Items = new List<SRMReceiptDetail>();
                    foreach (DataRow rdItem in dt.Rows)
                    {
                        var productDate = rdItem["ProductDate"].ConvertDateTime();
                        var expiryDate = rdItem["ExpiryDate"].ConvertDateTime();
                        SRMReceiptDetail requestDetail = new SRMReceiptDetail
                        {
                            OrderItemId = rdItem["OrderLineNo"].ConvertString(),
                            SkuId = rdItem["SkuId"].ConvertString(),
                            SkuStatus = rdItem["SkuStatus"].ConvertString(),
                            BatchNo = rdItem["BatchNo"].ConvertString(),
                            Qty = rdItem["QtyIn"].ConvertInt32(),
                            ProductDate = productDate.HasValue ? productDate.Value.ToString("yyyy-MM-dd") : null,
                            ExpiryDate = expiryDate.HasValue ? expiryDate.Value.ToString("yyyy-MM-dd") : null
                        };
                        request.Items.Add(requestDetail);
                    }
                    var oc = ApiFactory.Create("/gz_wms_asn_confirm").WithBody(request).LogResult();
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
