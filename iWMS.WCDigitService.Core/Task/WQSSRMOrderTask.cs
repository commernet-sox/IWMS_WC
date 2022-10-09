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
    /// 同步待检验的入库ASN
    /// </summary>
    public class WQSSRMOrderTask : CommonBaseTask
    {
        protected override bool Execute(TaskConfig config)
        {
            var typeData = new T_SRM_OrderData();
            var orderStatusData = new T_SYS_OrderSyncStatusData();
            //获取待检验的入库单
            var sql = $"SELECT TOP 10 * FROM SRM_Order A WHERE A.Status IN('55') AND A.UDF14='2' AND NOT EXISTS(SELECT 1 FROM SYS_Order_Sync_Status WHERE OrderId = A.BillId)";
            var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SRM_Order.TableName);
            //var ds = BusinessDbUtil.GetDataSet(sql, typeData.MES_PointTask.TableName);
            typeData.Merge(typeData.SRM_Order.TableName, ds);

            var sqlDtl = $"select * SRM_Order d INNER JOIN SRM_OrderDetail h ON h.BillId=d.BillId where d.BillId in ({string.Join("','", typeData.SRM_Order.Select(t => t.BillId))})";
            var dsDtl = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sqlDtl);
            var dt = dsDtl.Tables[0];

            if (typeData.SRM_Order.Count > 0 && dt.Rows.Count > 0)
            {
                foreach (var rItem in typeData.SRM_Order)
                {
                    var rdtl = dt.AsEnumerable().Where(t => t["BillId"].ConvertString() == rItem.BillId).FirstOrDefault();
                    //允许传多个

                    WQSSRMOrderRequest request = new WQSSRMOrderRequest();
                    request.LotNumber = rdtl["BatchNo"].ConvertString();
                    request.ItemNumber = rdtl["SkuId"].ConvertString();
                    request.ItemDesc = rdtl["UDF10"].ConvertString();
                    request.TransactionQuantity = rdtl["QtyPlan"].ConvertDecimal();
                    request.OrganizationCode = rdtl["UDF01"].ConvertString();
                    request.VendorNum = rItem.UDF03;
                    request.VendorName = rItem.UDF04;
                    request.TransactionDate = rItem.CreateDate;
                    

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
