using Common.DBCore;
using Common.ISource;
using FWCore;
using FWCore.TaskManage;
using iWMS.TypedData;
using iWMS.WCInterfaceService.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCInterfaceService.Core.Task
{
    /// <summary>
    /// 投料任务回传
    /// </summary>
    public class MESPointTask:CommonBaseTask
    {
        protected override bool Execute(TaskConfig config)
        {
            var typeData = new T_MES_PointTaskData();
            var orderStatusData = new T_SYS_OrderSyncStatusData();
            var sql = $"SELECT TOP 10 * FROM MES_PointTask A WHERE A.Status IN('40') AND A.DeliveryType = 'P2P' AND NOT EXISTS(SELECT 1 FROM SYS_Order_Sync_Status WHERE OrderId = A.DeliveryId) ";
            var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql,typeData.MES_PointTask.TableName);
            //var ds = BusinessDbUtil.GetDataSet(sql, typeData.MES_PointTask.TableName);
            typeData.Merge(typeData.MES_PointTask.TableName, ds);

            var sqlDtl = $"select * from MES_PointTaskDetail where DeliveryId in ('{string.Join("','",typeData.MES_PointTask.Select(t=>t.DeliveryId))}')";
            var dsDtl = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sqlDtl, typeData.MES_PointTaskDetail.TableName);
            //var dsDtl = BusinessDbUtil.GetDataSet(sqlDtl, typeData.MES_PointTaskDetail.TableName);
            typeData.Merge(typeData.MES_PointTaskDetail.TableName, dsDtl);
            if (typeData.MES_PointTask.Count > 0)
            {
                foreach (var pItem in typeData.MES_PointTask)
                {
                    var pdList = typeData.MES_PointTaskDetail.Where(t => t.DeliveryId == pItem.DeliveryId).ToList();
                    //允许传多个
                    List<MESPointRequest> requests = new List<MESPointRequest>();
                    foreach (var pdItem in pdList)
                    {
                        MESPointRequest request = new MESPointRequest();
                        request.DeliverySerial = pItem.DeliveryId;
                        request.Status = pItem.DeliveryStatus;
                        request.Appliance = pdItem.CID;
                        request.CreateBy = 0;
                        request.CreateTime = pItem.CreateDate;
                        request.ProductLineCode = pdItem.ProductLineCode;
                        request.ProductLineName = pdItem.ProductLineName;
                        request.PlanDate = pItem.PlanDate.ToString("yyyy-MM-dd");
                        request.FeedingMode = pItem.FeedingMode;

                        requests.Add(request);
                    }
                    var oc = ApiFactory.Create("SC/LES/SaveFeedingTaskToGMSComeback").WithBody(requests).LogResult();
                    Console.WriteLine(DateTime.Now);
                    //回传成功后更新主表信息
                    if (oc.IsSuccess)
                    {
                        var row = orderStatusData.SYS_Order_Sync_Status.NewSYS_Order_Sync_StatusRow();
                        row.OrderId = pItem.DeliveryId;
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

        }
    }
}
