using Common.ISource;
using FWCore;
using FWCore.TaskManage;
using iWMS.APILog;
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
    /// 产线MES投料任务更新
    /// </summary>
    public class UpdateProMESPointTask: CommonBaseTask
    {
        protected override bool Execute(TaskConfig config)
        {
            var typeData = new T_MES_PointTaskInterfaceData();
            var sql = $"select * from MES_PointTask_Interface where IsCompleted = 0 ";
            var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.MES_PointTask_Interface.TableName);
            typeData.Merge(typeData.MES_PointTask_Interface.TableName, ds);

            if (typeData.MES_PointTask_Interface.Count > 0)
            {
                foreach (var item in typeData.MES_PointTask_Interface)
                {
                    //定时任务来做
                    //先找投料点看有没有映射库位,有的话就是C2库位，按库位顺序找投料任务
                    string mapLoc = @"SELECT MappingLoc FROM MES_Point WHERE ProductLineCode='{0}' AND PointCode='{1}'";
                    var mappingLoc = GlobalContext.Resolve<ISource_SQLHelper>().ExecuteScalar(string.Format(mapLoc, item.ProductLineCode, item.PointCode)).ToString();

                    if (!string.IsNullOrWhiteSpace(mappingLoc))
                    {
                        var locs = mappingLoc.Split(',');
                        string LocationSql = $"SELECT LocationId,CID,WHAreaId,PutAwayOrder,Property1 AS ProductLineCode,Property2 AS PointCode FROM COM_Location where Status=1 and LocationId in ('{string.Join("','",locs)}') order by PutAwayOrder asc";
                        var Locationtable = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(LocationSql).Tables[0];
                        foreach (DataRow it in Locationtable.Rows)
                        {
                            var ProductLineCode = it["ProductLineCode"].ConvertString();
                            var pointCode = it["PointCode"].ConvertString();
                            var pointTaskDetailSql = $"SELECT TOP 1 A.DeliveryId FROM MES_PointTask A INNER JOIN MES_PointTaskDetail B ON A.DeliveryId=B.DeliveryId WHERE A.DeliveryType = 'P2P' AND A.Status = '20' AND B.ProductLineCode = '{ProductLineCode}' AND B.PointCode = '{pointCode}' ORDER BY A.ModifyDate ASC";
                            var dsPoint = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(pointTaskDetailSql);
                            if (dsPoint != null && dsPoint.Tables[0].Rows.Count > 0)
                            {
                                var dt = dsPoint.Tables[0];

                                var updateSql = @"UPDATE MES_PointTask SET Status='30',MESTaskId='{1}',OrigSystem='{2}',ModifyBy='System',ModifyDate=GETDATE(),Version=Version+1 WHERE DeliveryId='{0}' And Status=='20';
										  UPDATE MES_PointTaskDetail SET Status='30',ModifyBy='System',ModifyDate=GETDATE(),Version=Version+1 WHERE DeliveryId='{0}' And Status=='20'";
                                try
                                {
                                    GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(string.Format(updateSql, dt.Rows[0]["DeliveryId"].ConvertString(), item.TaskId, item.Ex1));
                                    break;

                                }
                                catch (Exception ex)
                                {
                                    NLogUtil.WriteError(ex.ToString());
                                }
                            }
                            else
                            {
                                continue;
                            }

                        }
                    }
                    else
                    {
                        //这里要不要加  A.DeliveryType = 'P2P'
                        var pointTaskDetailSql = $"SELECT TOP 1 A.DeliveryId FROM MES_PointTask A INNER JOIN MES_PointTaskDetail B ON A.DeliveryId=B.DeliveryId WHERE A.DeliveryType = 'P2P' AND A.Status = '20' AND B.ProductLineCode = '{item.ProductLineCode}' AND B.PointCode = '{item.PointCode}' ORDER BY A.ModifyDate ASC";
                        var dsPoint = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(pointTaskDetailSql);
                        if (dsPoint != null && dsPoint.Tables[0].Rows.Count > 0)
                        {
                            var dt = dsPoint.Tables[0];

                            var updateSql = @"UPDATE MES_PointTask SET Status='30',MESTaskId='{1}',OrigSystem='{2}',ModifyBy='System',ModifyDate=GETDATE(),Version=Version+1 WHERE DeliveryId='{0}' And Status=='20';
										  UPDATE MES_PointTaskDetail SET Status='30',ModifyBy='System',ModifyDate=GETDATE(),Version=Version+1 WHERE DeliveryId='{0}' And Status=='20'";
                            try
                            {
                                GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(string.Format(updateSql, dt.Rows[0]["DeliveryId"].ConvertString(), item.TaskId, item.Ex1));

                            }
                            catch (Exception ex)
                            {
                                NLogUtil.WriteError(ex.ToString());
                            }
                        }
                        else
                        {
                            NLogUtil.WriteError("参数错误，未找到投料任务");
                            //return new Outcome(400, "参数错误，未找到投料任务");
                        }
                    }
                }
                    
            }
            return true;
        }
    }
}
