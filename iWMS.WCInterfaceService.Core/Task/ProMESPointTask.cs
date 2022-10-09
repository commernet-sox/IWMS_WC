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
    /// 产线MES投料任务回传
    /// </summary>
    public class ProMESPointTask : CommonBaseTask
    {
        protected override bool Execute(TaskConfig config)
        {
            var typeData = new T_MES_PointTaskData();
            var orderStatusData = new T_SYS_OrderSyncStatusData();
            var pullTaskData = new T_MES_PullTaskData();
            var comSkuData = new T_COM_SKUData();
            var mesCategoryData = new T_MES_CategoryData();
            var sql = $"SELECT TOP 10 * FROM MES_PointTask A WHERE A.Status IN('40') AND A.OrigSystem = 'MES' AND NOT EXISTS(SELECT 1 FROM SYS_Order_Sync_Status WHERE OrderId = A.MESTaskId) ";
            var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.MES_PointTask.TableName);
            //var ds = BusinessDbUtil.GetDataSet(sql, typeData.MES_PointTask.TableName);
            typeData.Merge(typeData.MES_PointTask.TableName, ds);

            var sqlDtl = $"select * from MES_PointTaskDetail where DeliveryId in ('{string.Join("','", typeData.MES_PointTask.Select(t => t.DeliveryId))}')";
            var dsDtl = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sqlDtl, typeData.MES_PointTaskDetail.TableName);
            //var dsDtl = BusinessDbUtil.GetDataSet(sqlDtl, typeData.MES_PointTaskDetail.TableName);
            typeData.Merge(typeData.MES_PointTaskDetail.TableName, dsDtl);

            var pullTaskSql = $"select * from MES_PullTask where TaskId in ('{string.Join("','", typeData.MES_PointTaskDetail.Select(t => t.PullNumber))}')";
            var pullTask = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(pullTaskSql, pullTaskData.MES_PullTask.TableName);
            pullTaskData.Merge(pullTaskData.MES_PullTask.TableName, pullTask);

            var comSkuSql = $"select * from COM_SKU where SkuId in ('{string.Join("','", typeData.MES_PointTaskDetail.Select(t => t.SkuId))}')";
            var comSku = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(comSkuSql, comSkuData.COM_SKU.TableName);
            comSkuData.Merge(comSkuData.COM_SKU.TableName, comSku);

            var mesCategorySql = $"select * from MES_Category where CategoryCode in ('{string.Join("','", comSkuData.COM_SKU.Select(t => t.CategoryId))}')";
            var mesCategory = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(mesCategorySql, mesCategoryData.MES_Category.TableName);
            mesCategoryData.Merge(mesCategoryData.MES_Category.TableName, mesCategory);


            if (typeData.MES_PointTask.Count > 0)
            {
                foreach (var pItem in typeData.MES_PointTask)
                {
                    var pdList = typeData.MES_PointTaskDetail.Where(t => t.DeliveryId == pItem.DeliveryId).ToList();
                    //允许传多个
                    ProMESPointRequest requests = new ProMESPointRequest();
                    requests.TaskId = pItem.DeliveryId;
                    requests.ProductlineCode = pdList[0].ProductLineCode;
                    requests.OperationCode = pdList[0].StationCode;
                    requests.LocationId = pdList[0].PointCode;
                    requests.PalletId = pdList[0].CID;
                    foreach (var pdItem in pdList)
                    {
                        ProMESPointRequestDetail request = new ProMESPointRequestDetail();
                        var pullTaskRow = pullTaskData.MES_PullTask.Where(t => t.TaskId == pdItem.PullNumber).FirstOrDefault();
                        var comSkuRow = comSkuData.COM_SKU.Where(t => t.SkuId == pdItem.SkuId).FirstOrDefault();
                        var mesCategoryRow = mesCategoryData.MES_Category.Where(t => t.CategoryCode == comSkuRow.CategoryId).FirstOrDefault();

                        request.ProductCode = pullTaskRow.ProductCode;
                        request.LocationId = pdItem.PointCode;
                        request.MaterialBatch = pdItem.BatchNum;
                        request.MaterialCode = pdItem.SkuId;
                        request.MaterialName = pdItem.SkuName;
                        request.Qty = pdItem.Qty;
                        request.vendorCode = pullTaskRow.SupplierCode;
                        request.IsKeyParts = mesCategoryRow.KeyFlag == "1" ? 1 : 0 ;
                        request.IsBatch = mesCategoryRow.InfoType=="1"?1:(mesCategoryRow.InfoType == "0"?2:0);
                        request.Memo = "";
                        request.TrimMark = "";
                        requests.Items.Add(request);
                    }
                    var oc = ApiFactory.CreateMES("API/FeedingTaskReturn").WithBody(requests).LogResult();
                    Console.WriteLine(DateTime.Now);
                    //回传成功后更新主表信息
                    if (oc.IsSuccess)
                    {
                        var row = orderStatusData.SYS_Order_Sync_Status.NewSYS_Order_Sync_StatusRow();
                        row.OrderId = pItem.MESTaskId;
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
