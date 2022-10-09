using Common.ISource;
using FWCore;
using FWCore.TaskManage;
using iWMS.APILog;
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
    /// 物料需求时间回传
    /// </summary>
    public class MESPullTask : CommonBaseTask
    {
        protected override bool Execute(TaskConfig config)
        {
            var typeData = new T_MES_PullTaskTimeData();
            var pullCalTimeData=new T_MES_PullCalTimeData();
            var sql = $"SELECT TOP (100) * FROM MES_PullCalTime WHERE UploadFlag=0";
            var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, pullCalTimeData.MES_PullCalTime.TableName);
            //var ds = BusinessDbUtil.GetDataSet(sql, typeData.MES_PointTask.TableName);
            pullCalTimeData.Merge(pullCalTimeData.MES_PullCalTime.TableName, ds);

            if (pullCalTimeData.MES_PullCalTime.Count > 0)
            {
                foreach (var item in pullCalTimeData.MES_PullCalTime)
                {

                    MESPullTaskRequest request = new MESPullTaskRequest();

                    request.PlanDate = item.PlanDate.ToString("yyyy-MM-dd HH:mm:ss");
                    request.ProductLineCode = item.ProductLineCode;
                    request.ProductLineName = item.ProudctLineName;
                    request.TaskCode = item.PullNumber;
                    request.MaterialCode = item.SkuId;
                    request.MaterialName = item.SkuName;
                    request.NeedNum = item.Qty.ConvertString();
                    request.InputEndPlanTime = item.PlanInputDate.ToString("yyyy-MM-dd HH:mm:ss");
                    request.TransEndPlanTime = item.PlanTransDate.ToString("yyyy-MM-dd HH:mm:ss");
                    request.DeliveryEndPlanTime = item.PlanDeliveryDate.ToString("yyyy-MM-dd HH:mm:ss");
                    request.TransTime = item.SendTime.ToString("yyyy-MM-dd HH:mm:ss");
                    //request.FeedingMode=item.FeedingMode;
                    request.WaveNum=item.WaveNum;
                    request.BatchNum = item.BatchNum;
                    request.PackSort = item.PackSort.ToString();
                    request.FeedingPointCode = item.PointCode;
                    request.FeedingPointName = item.PointName;
                    request.CarGroupCode = item.CarGroupCode;
                    request.CarGroupName = item.CarGroupName;
                    var oc = ApiFactory.Create("SC/LES/GmsTransportPostbackSave").WithBody(new[] { request }).LogResult();
                    Console.WriteLine(DateTime.Now);
                    //回传成功后更新主表信息
                    if (oc.IsSuccess)
                    {
                        item.UploadFlag = 1;

                        var row = typeData.MES_PullTaskTime.NewMES_PullTaskTimeRow();
                        row.OrderNo = item.OrderNo;
                        row.PlanDate = item.PlanDate;
                        row.ProductLineCode = item.ProductLineCode;
                        row.ProudctLineName = item.ProudctLineName;
                        row.PullNumber = item.PullNumber;
                        row.SkuId = item.SkuId;
                        row.SkuName = item.SkuName;
                        row.Qty = item.Qty;
                        row.PlanInputDate = item.PlanInputDate;
                        row.PlanTransDate = item.PlanTransDate;
                        row.PlanDeliveryDate = item.PlanDeliveryDate;
                        row.SendTime = item.SendTime;
                        row.FeedingMode = item.FeedingMode;
                        row.WaveNum = item.WaveNum;
                        row.BatchNum = item.BatchNum;
                        row.PackSort = item.PackSort;
                        row.PointCode = item.PointCode;
                        row.PointName = item.PointName;
                        row.CarGroupCode = item.CarGroupCode;
                        row.CarGroupName = item.CarGroupName;
                        typeData.MES_PullTaskTime.AddMES_PullTaskTimeRow(row);
                        GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
                        GlobalContext.Resolve<ISource_SQLHelper>().Save(pullCalTimeData);
                        typeData.AcceptChanges();
                    }
                    else//回传失败记录日志
                    {
                        NLogUtil.WriteError($"物料需求时间:{item.PullNumber} 单号回传失败");
                    }
                }
            }

            return true;
        }
    }
}
