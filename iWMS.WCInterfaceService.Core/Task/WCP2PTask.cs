using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Common.DBCore;
using Common.ISource;
using FWCore;
using FWCore.TaskManage;
using iWMS.APILog;
using iWMS.WCInterfaceService.Core.Util;

namespace iWMS.WCInterfaceService.Core.Task
{
    /// <summary>
    /// 投料任务分解
    /// </summary>
	public class WCP2PTask : CommonBaseTask
    {
        //点对点
        private const string P2PTaskSql = @"SELECT * from ((SELECT TOP(100) * FROM MES_PointTask WHERE Status='10' AND DeliveryType='P2P' ORDER BY CreateDate) pt INNER JOIN MES_PointTaskDetail ptd ON pt.DeliveryId=ptd.DeliveryId )";
        //多点
        private const string M2PTaskSql = @"SELECT * FROM ((SELECT TOP(100) * FROM MES_PointTask WHERE Status='10' AND DeliveryType='M2P' ORDER BY CreateDate) pt INNER JOIN MES_PointTaskDetail ptd ON pt.DeliveryId=ptd.DeliveryId )";
        //获取存储区和产线区的库位
        //private const string LocationSql = @"SELECT LocationId,CID,WHAreaId,Property1 AS ProductLineCode,Property2 AS PointCode FROM COM_Location where Status=1";
        //更新投料任务状态信息
        private const string UpdatePointTask = @"UPDATE MES_PointTask SET Status='20',ModifyBy='System',ModifyDate=GETDATE(),Version=Version+1,Memo='' WHERE DeliveryId='{0}'  And Status='10';
          UPDATE MES_PointTaskDetail SET Status='20',ModifyBy='System',ModifyDate=GETDATE(),Version=Version+1 WHERE DeliveryId='{0}' And Status='10'";

        //获取源库位数据
        private const string StartLocationSql = @"SELECT a.LocationId,b.CID,a.WHAreaId,a.Property1 AS ProductLineCode,a.Property2 AS PointCode 
FROM COM_Location a inner join INV_BAL b on a.LocationId=b.LocationId
where a.Status=1 and b.Qty>0 and b.CID in ('{0}') and a.WHAreaId='C1'
group by a.LocationId,b.CID,a.WHAreaId,a.Property1,a.Property2";

        /// <summary>
        /// 获取目标库位数据
        /// </summary>
        private const string EndLocationSql = @"
SELECT a.LocationId,a.WHAreaId,a.CID,a.PutAwayOrder,a.Property1 AS ProductLineCode,a.Property2 AS PointCode,cast(0 as decimal(18,4)) qty into #tmp FROM COM_Location a where a.Property1 in ('{0}') and a.Property2 in ('{1}') and a.Status='1'

update t set t.qty=(select sum(qty) from INV_BAL where LocationId=t.LocationId)
from #tmp t 

select * from #tmp";
        /// <summary>
        /// 获取投料点是否存在映射
        /// </summary>
        private const string mapLocationSql = @"SELECT MappingLoc FROM MES_Point WHERE ProductLineCode='{0}' AND PointCode='{1}'";
        /// <summary>
        /// 获取投料点映射的起始库位
        /// </summary>
        private const string StartMapLocationSql = @"
			SELECT a.LocationId,a.WHAreaId,a.CID,a.Property1 AS ProductLineCode,a.Property2 AS PointCode,cast(0 as decimal(18,4)) qty,a.PutAwayOrder into #tmp FROM COM_Location a where a.LocationId in ('{0}') and a.Status='1' and a.WHAreaId='C2' 

			update t set t.qty=(select sum(qty) from INV_BAL where LocationId=t.LocationId)
			from #tmp t 

			select * from #tmp where qty>0 order by PutAwayOrder asc";


        protected override bool Execute(TaskConfig config)
        {
            
            try
            {
                lock (LockObject)
                {
                    P2PTask();
                    M2PTask();
                }
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
            }
            return true;
        }
        /// <summary>
        /// 点对点具体业务逻辑
        /// </summary>
        private static void P2PTask()
        {
            DataTable sLocationtable = new DataTable();
            DataTable eLocationtable = new DataTable();
            //var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SRM_Receipt.TableName);

            DataTable P2Ptable = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(P2PTaskSql).Tables[0];
            if (P2Ptable != null && P2Ptable.Rows.Count > 0)
            {
                //获取移库任务托盘信息
                var cids = P2Ptable.AsEnumerable().Select(t => t["CID"].ToString()).Distinct().ToList();
                sLocationtable = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(String.Format(StartLocationSql, String.Join("','",cids) ) ).Tables[0];

                var productlinecodes = P2Ptable.AsEnumerable().Select(t => t["ProductLineCode"].ToString()).Distinct().ToList();
                var pointcodes = P2Ptable.AsEnumerable().Select(t => t["PointCode"].ToString()).Distinct().ToList();
                eLocationtable=GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(String.Format(EndLocationSql, String.Join("','", productlinecodes), String.Join("','", pointcodes))).Tables[0];


                var groups = P2Ptable.AsEnumerable().GroupBy(item => item["DeliveryId"].ConvertString());

                
                foreach (var group in groups)
                {
                    //按投料单号逐一生成上架任务
                    List<DataRow> YKList = new List<DataRow>();
                    List<DataRow> FHList = new List<DataRow>();
                    bool IsCompleted = true;
                    var DeliveryId = group.Key;
                    var groupList = group.ToList();
                    foreach (DataRow row in groupList)
                    {
                        var ProductLineCode = row["ProductLineCode"].ConvertString();
                        var PointCode = row["PointCode"].ConvertString();

                        var locRow = eLocationtable.AsEnumerable().Where(t => t["ProductLineCode"].ConvertString() == ProductLineCode && t["PointCode"].ConvertString() == PointCode).FirstOrDefault();
                        
                        if (locRow != null)
                        {
                            //如果目标库位库存大于0，分解失败
                            if (locRow["qty"].ConvertDecimal() > 0)
                            {
                                var updatePTDetail = $"UPDATE MES_PointTask SET Memo='{"投料单明细目标库位有库存"}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
                                GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePTDetail);
                                NLogUtil.WriteError($"{DeliveryId} 投料单明细目标库位有库存");
                                IsCompleted = false;
                                break;
                            }

                            var WHAreaId = locRow["WHAreaId"].ConvertString();
                            if (WHAreaId.Contains("C"))
                            {
                                YKList.Add(row);
                            }
                            else if (WHAreaId.Contains("P"))
                            {
                                FHList.Add(row);
                            }
                            else
                            {
                            }
                        }
                        else//未匹配到的库位
                        {
                            var updatePTDetail = $"UPDATE MES_PointTask SET Memo='{"投料单明细未匹配到库位"}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
                            GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePTDetail);
                            NLogUtil.WriteError($"{DeliveryId} 投料单明细未匹配到库位");
                            IsCompleted = false;
                            break;
                        }
                        
                    }
                    //处理移库和发货任务
                    if (YKList.Count > 0)
                    {
                        IsCompleted = YKTask(YKList, sLocationtable,eLocationtable, IsCompleted);
                    }
                    if (FHList.Count > 0)
                    {
                        IsCompleted = P2PFHTask(FHList, sLocationtable, eLocationtable, IsCompleted, DeliveryId,true);
                    }
                    if (IsCompleted)
                    {
                        //更新投料任务状态
                        try
                        {
                            GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(string.Format(UpdatePointTask, DeliveryId));
                            //BusinessDbUtil.ExecuteNonQuery(string.Format(UpdatePointTask, DeliveryId));
                        }
                        catch (Exception ex)
                        {
                            var updatePTDetail = $"UPDATE MES_PointTask SET Memo='{ex.ToString()}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
                            GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePTDetail);
                            NLogUtil.WriteError(ex.ToString());
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 多点具体业务逻辑
        /// </summary>
        private static void M2PTask()
        {
            DataTable sLocationtable = new DataTable();
            DataTable eLocationtable = new DataTable();

            DataTable M2Ptable =GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(M2PTaskSql).Tables[0];
            if (M2Ptable != null && M2Ptable.Rows.Count > 0)
            {

                //获取移库任务托盘信息
                var cids = M2Ptable.AsEnumerable().Select(t => t["CID"].ToString()).Distinct().ToList();
                sLocationtable = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(String.Format(StartLocationSql, String.Join("','", cids))).Tables[0];

                var productlinecodes = M2Ptable.AsEnumerable().Select(t => t["ProductLineCode"].ToString()).Distinct().ToList();
                var pointcodes = M2Ptable.AsEnumerable().Select(t => t["PointCode"].ToString()).Distinct().ToList();
                eLocationtable = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(String.Format(EndLocationSql, String.Join("','", productlinecodes), String.Join("','", pointcodes))).Tables[0];

                var groups = M2Ptable.AsEnumerable().OrderBy(t => t["DockingOrder"].ConvertInt32() ).GroupBy(item => new { DeliveryId = item["DeliveryId"].ConvertString() });

                foreach (var group in groups)
                {
                    //按投料单号逐一生成下架任务
                    List<DataRow> FHList = new List<DataRow>();
                    List<DataRow> QTList = new List<DataRow>();
                    bool IsCompleted = true;
                    var DeliveryId = group.Key.DeliveryId;
                    TypedData.T_MES_M2PTaskData t_MES_M2P = new TypedData.T_MES_M2PTaskData();
                    var pointTaskSql = $"select * from MES_PointTask where DeliveryId='{DeliveryId}'";
                    var pointTaskDetailSql = $"select * from MES_PointTaskDetail where DeliveryId='{DeliveryId}'";
                    var pointTaskSqlDS = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(pointTaskSql, t_MES_M2P.MES_PointTask.TableName);
                    t_MES_M2P.Merge(t_MES_M2P.MES_PointTask.TableName, pointTaskSqlDS);

                    var pointTaskDetailSqlDS = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(pointTaskDetailSql, t_MES_M2P.MES_PointTaskDetail.TableName);
                    t_MES_M2P.Merge(t_MES_M2P.MES_PointTaskDetail.TableName, pointTaskDetailSqlDS);
                    //var PointCode = group.Key.PointCode;
                    var groupList = group.ToList().GroupBy(item => item["PointCode"].ConvertString());
                    for (var i = 0; i < groupList.ToList().Count(); i++)
                    {
                        var pointGroup = groupList.ToList();
                        //var PointCode = pointGroup[i].Key;
                        //第一个作为发货任务处理
                        if (i == 0)
                        {
                            FHList.Add((DataRow)pointGroup[i].ToList().FirstOrDefault());
                        }
                        //后续作为其他任务处理
                        else
                        {
                            QTList.Add((DataRow)pointGroup[i].ToList().FirstOrDefault());
                        }
                    }
                    //处理发货和其他任务
                    var lastLoc = "";
                    if (FHList.Count > 0)
                    {
                        IsCompleted = FHTask(FHList, sLocationtable, eLocationtable, IsCompleted, DeliveryId, false, t_MES_M2P);
                        if (IsCompleted)
                        {
                            lastLoc = t_MES_M2P.TRM_OrderTaskDetail.FirstOrDefault().ToLoc;
                        }
                        
                    }
                    if (QTList.Count > 0 && IsCompleted)
                    {
                        //TypedData.T_TRM_TaskOtherData _OtherData = new TypedData.T_TRM_TaskOtherData();
                        
                        foreach (var item in QTList)
                        {
                            var fromLocRow = sLocationtable.AsEnumerable().Where(t => t["CID"].ConvertString() == item["CID"].ConvertString()).FirstOrDefault();
                            var toLocRow = eLocationtable.AsEnumerable().Where(t => t["ProductLineCode"].ConvertString() == item["ProductLineCode"].ConvertString() && t["PointCode"].ConvertString() == item["PointCode"].ConvertString()).FirstOrDefault();
                            var taskIds = GlobalContext.Resolve<ISource_SQLHelper>().ExecuteScalar($"DECLARE @Id VARCHAR(30) exec SP_GEN_TASKID 'TO',{Commons.StoreId},{Commons.WarehouseId},@Id OUTPUT SELECT @Id").ConvertString();

                            
                            var trm_TaskOther_Row = t_MES_M2P.TRM_TaskOther.NewTRM_TaskOtherRow();
                            trm_TaskOther_Row.TaskId = taskIds;
                            trm_TaskOther_Row.TaskType = "50";
                            trm_TaskOther_Row.Status = "10";
                            trm_TaskOther_Row.Priority = toLocRow["PutAwayOrder"].ConvertInt32();
                            trm_TaskOther_Row.FromLoc = fromLocRow["LocationId"].ConvertString();
                            trm_TaskOther_Row.ToLoc = toLocRow["LocationId"].ConvertString();
                            lastLoc = trm_TaskOther_Row.ToLoc;
                            trm_TaskOther_Row.CID = item["CID"].ConvertString();
                            trm_TaskOther_Row.IsAgvCompleted = 0;
                            trm_TaskOther_Row.Version = 0;
                            trm_TaskOther_Row.CreateBy = "System";
                            trm_TaskOther_Row.CreateDate = DateTime.Now;
                            t_MES_M2P.TRM_TaskOther.AddTRM_TaskOtherRow(trm_TaskOther_Row);

                            var updatePTDetail = $"UPDATE MES_PointTask SET Memo='{"保存巡线其他任务失败:" + DeliveryId}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
                            //try
                            //{
                            //    GlobalContext.Resolve<ISource_SQLHelper>().Save(t_MES_M2P);
                            //}
                            //catch (Exception)
                            //{
                            //    IsCompleted = false;
                            //    GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePTDetail);
                            //    NLogUtil.WriteError("保存巡线其他任务失败:" + DeliveryId);
                            //    continue;
                            //}
                            //保存任务成功后回写投料明显表WmsTaskId
                            IsCompleted = true;
                            var taskDetailRow = t_MES_M2P.MES_PointTaskDetail.Where(t => t.DeliveryId == DeliveryId && t.FlowNo== item["FlowNo"].ConvertLong()).FirstOrDefault();

                            taskDetailRow.WMSTaskId = taskIds;

                            //updatePTDetail = $"update MES_PointTaskDetail set WMSTaskId='{taskIds}' where FlowNo={item["FlowNo"].ConvertInt32()} and DeliveryId='{DeliveryId}'";
                            //GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePTDetail);

                        }
                    }
                    if (IsCompleted)
                    {
                        //加一个退空任务
                        string toLocSql = @"select ParmValue from SYS_Parameter where ParmId='POTHER003'";
                        var toLoc = GlobalContext.Resolve<ISource_SQLHelper>().ExecuteScalar(toLocSql).ConvertString();
                        string toPrioritySql = $"select PutAwayOrder from COM_Location where LocationId='{toLoc}'";
                        var toPriority = GlobalContext.Resolve<ISource_SQLHelper>().ExecuteScalar(toPrioritySql).ConvertInt32();
                        var trm_TaskOther_Row1 = t_MES_M2P.TRM_TaskOther.NewTRM_TaskOtherRow();
                        trm_TaskOther_Row1.TaskId = DeliveryId;


                        trm_TaskOther_Row1.TaskType = "50";
                        trm_TaskOther_Row1.Status = "10";
                        trm_TaskOther_Row1.Priority = toPriority;
                        trm_TaskOther_Row1.FromLoc = lastLoc;
                        trm_TaskOther_Row1.ToLoc = toLoc;
                        trm_TaskOther_Row1.CID = QTList[0]["CID"].ConvertString();
                        trm_TaskOther_Row1.IsAgvCompleted = 0;
                        trm_TaskOther_Row1.Ex10 = DeliveryId;
                        trm_TaskOther_Row1.Version = 0;
                        trm_TaskOther_Row1.CreateBy = "System";
                        trm_TaskOther_Row1.CreateDate = DateTime.Now;
                        t_MES_M2P.TRM_TaskOther.AddTRM_TaskOtherRow(trm_TaskOther_Row1);
                    }
                    
                    if (IsCompleted)
                    {
                        //更新巡线任务状态
                        try
                        {
                            GlobalContext.Resolve<ISource_SQLHelper>().Save(t_MES_M2P);

                            GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(string.Format(UpdatePointTask, DeliveryId));
                        }
                        catch (Exception ex)
                        {
                            var updatePTDetail = $"UPDATE MES_PointTask SET Memo='{"保存巡线任务失败：" + ex.ToString()}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
                            GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePTDetail);
                            NLogUtil.WriteError("保存巡线任务失败：" + ex.ToString());
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 移库任务
        /// </summary>
        /// <param name="YKList"></param>
        /// <param name="Locationtable"></param>
        /// <param name="IsCompleted"></param>
        /// <returns></returns>
        private static bool YKTask(List<DataRow> YKList,DataTable sLocationtable, DataTable eLocationtable, bool IsCompleted)
        {
            TypedData.T_WRM_MoveLocData moveLocData = new TypedData.T_WRM_MoveLocData();
            //获取BillId
            var mov_BillId = GlobalContext.Resolve<ISource_BillHelper>().GetBillIdCustom("MOV", Commons.StoreId, Commons.WarehouseId);
            //var mov_BillId = BusinessDbUtil.ExecuteScalar($"DECLARE @Id VARCHAR(30) exec SP_GEN_ID 'MOV',{StorerId},{WarehouseId},@Id OUTPUT SELECT @Id").ConvertString();

            //按库存明细生成对呀的移库明细
            var fromLocRow = sLocationtable.AsEnumerable().Where(t => t["CID"].ConvertString() == YKList[0]["CID"].ConvertString()).FirstOrDefault();
            var toLocRow = eLocationtable.AsEnumerable().Where(t => t["ProductLineCode"].ConvertString() == YKList[0]["ProductLineCode"].ConvertString() && t["PointCode"].ConvertString() == YKList[0]["PointCode"].ConvertString()).FirstOrDefault();
            if (fromLocRow == null || toLocRow == null)
            {
                //移库起始点库存不足
                var updatePTDetail = $"UPDATE MES_PointTask SET Memo='{"起始点或终点库位为空"}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{YKList[0]["DeliveryId"].ConvertString()}' And Status='10'";
                GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePTDetail);
                NLogUtil.WriteError("起始点或终点库位为空");
                IsCompleted = false;
                return IsCompleted;
            }
            DataSet ds_Stock = Get_MoveStockList(fromLocRow["locationId"].ConvertString(),"","", "", 0);
            #region 验证库存
            //if (ds_Stock.Tables.Count == 0 || ds_Stock.Tables[0].Rows.Count == 0)
            //{
            //    IsCompleted = false;
            //    NLogUtil.WriteError($"物料:[{item["SkuId"].ConvertString()}],库存验证失败,请刷新后重试!");
            //    return IsCompleted;
            //}
            //else
            //{
            //    if (ds_Stock.Tables[0].Rows[0]["QtyValid"].ConvertDecimal() < decimal.Parse(item["Qty"].ConvertString()))
            //    {
            //        IsCompleted = false;
            //        NLogUtil.WriteError($"物料:[{ item["SkuId"].ConvertString() }],库存不足,请刷新后重试!");
            //        return IsCompleted;
            //    }

            //}
            #endregion
            #region 组装数据
            DataTable dt = ds_Stock.Tables[0];
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    var wrm_MoveLocDetail_Row = moveLocData.WRM_MoveLocDetail.NewWRM_MoveLocDetailRow();
                    wrm_MoveLocDetail_Row.BillId = mov_BillId;
                    wrm_MoveLocDetail_Row.StorerId = Commons.StoreId;
                    wrm_MoveLocDetail_Row.SkuId = item["SkuId"].ConvertString();
                    wrm_MoveLocDetail_Row.FromBatchNo = item["BatchNo"].ConvertString();
                    wrm_MoveLocDetail_Row.FromSkuStatus = "AVL";
                    wrm_MoveLocDetail_Row.FromWHAreaId = fromLocRow["WHAreaId"].ConvertString();
                    wrm_MoveLocDetail_Row.FromCID = item["CID"].ConvertString();
                    wrm_MoveLocDetail_Row.FromBoxCode = item["BoxCode"].ConvertString();
                    wrm_MoveLocDetail_Row.FromLoc = fromLocRow["locationId"].ConvertString();
                    wrm_MoveLocDetail_Row.ToBatchNo = item["BatchNo"].ConvertString();
                    wrm_MoveLocDetail_Row.ToSkuStatus = "AVL";
                    wrm_MoveLocDetail_Row.ToCID = toLocRow["CID"].ConvertString();
                    wrm_MoveLocDetail_Row.ToBoxCode = item["BoxCode"].ConvertString();
                    //通过产线编码和投料点编码获取对应库位
                    wrm_MoveLocDetail_Row.ToLoc = toLocRow["LocationId"].ConvertString();
                    wrm_MoveLocDetail_Row.ToWHAreaId = toLocRow["WHAreaId"].ConvertString();
                    wrm_MoveLocDetail_Row.FromQty = item["QtyValid"].ConvertDecimal();
                    //item["Qty"].ConvertDecimal();  移库数量都取库存表的数量，整托移过去
                    wrm_MoveLocDetail_Row.ToQty = item["QtyValid"].ConvertDecimal();
                    wrm_MoveLocDetail_Row.PackageCode = item["PackageCode"].ConvertString();
                    wrm_MoveLocDetail_Row.PackageQty = item["PackageQty"].ConvertDecimal();
                    wrm_MoveLocDetail_Row.Version = 0;
                    wrm_MoveLocDetail_Row.CreateBy = "System";
                    wrm_MoveLocDetail_Row.CreateDate = DateTime.Now;
                    moveLocData.WRM_MoveLocDetail.AddWRM_MoveLocDetailRow(wrm_MoveLocDetail_Row);
                }
            }
            else
            {
                //移库起始点库存不足
                var updatePTDetail = $"UPDATE MES_PointTask SET Memo='{"移库起始点库存不足"}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{YKList[0]["DeliveryId"].ConvertString()}' And Status='10'";
                GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePTDetail);
                NLogUtil.WriteError("移库起始点库存不足");
                IsCompleted = false;
                return false;
            }
            
            var goodsCount = (from t in moveLocData.WRM_MoveLocDetail.AsEnumerable()
                              group t by new { t1 = t.Field<string>("SkuId") } into m
                              select new
                              {
                                  SkuId = m.Key.t1
                              }).ToList().Count;
            var skuCount = moveLocData.WRM_MoveLocDetail.AsEnumerable().Sum(t => t.ToQty.ConvertDecimal().ConvertInt32());
            var wrm_MoveLoc_Row = moveLocData.WRM_MoveLoc.NewWRM_MoveLocRow();
            wrm_MoveLoc_Row.BillId = mov_BillId;
            wrm_MoveLoc_Row.OrderType = "Move";//投料出库P2P-OUT
            wrm_MoveLoc_Row.OrigOrderType = "0";
            wrm_MoveLoc_Row.OrderFlag = "Interface";
            wrm_MoveLoc_Row.WorkMode = "AGV";
            wrm_MoveLoc_Row.WarehouseId = Commons.WarehouseId;
            wrm_MoveLoc_Row.StorerId = Commons.StoreId;
            wrm_MoveLoc_Row.DestStorerId = Commons.StoreId;
            wrm_MoveLoc_Row.OrderDate = DateTime.Now;
            wrm_MoveLoc_Row.Status = "10";
            wrm_MoveLoc_Row.GoodsCount = goodsCount;
            wrm_MoveLoc_Row.SkuCount = skuCount;
            wrm_MoveLoc_Row.Version = 0;
            wrm_MoveLoc_Row.CreateBy = "System";
            wrm_MoveLoc_Row.CreateDate = DateTime.Now;
            moveLocData.WRM_MoveLoc.AddWRM_MoveLocRow(wrm_MoveLoc_Row);
            #endregion
            string[] psql = new string[]
            {
                $"EXEC SP_WRM_MoveLoc_Check  '{mov_BillId}','{"System"}','{"System"}','{"System"}'"
            };
            var res = GlobalContext.Resolve<ISource_SQLHelper>().SaveProc(moveLocData, psql[0]);
            //BusinessDbUtil.SaveProc(moveLocData, psql);
            if (!res.IsSuccess)
            {
                IsCompleted = false;
                var updatePTDetail = $"UPDATE MES_PointTask SET Memo='调用审核存储过程失败:{res.ErrMsg}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{YKList[0]["DeliveryId"].ConvertString()}' And Status='10'";
                GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePTDetail);
                NLogUtil.WriteError("调用审核存储过程失败:" + res.ErrMsg);
            }
            else
            {
                //保存任务成功后回写投料明显表WmsTaskId
                var updatePTDetail = $"UPDATE MES_PointTask SET WMSMoveId='{mov_BillId}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{YKList[0]["DeliveryId"].ConvertString()}' update MES_PointTaskDetail set WMSMoveId='{mov_BillId}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' where FlowNo={YKList[0]["FlowNo"].ConvertInt32()} and DeliveryId='{YKList[0]["DeliveryId"].ConvertString()}' And Status='10'";
                GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePTDetail);
                IsCompleted = true;
            }
            return IsCompleted;
        }
        /// <summary>
        /// 单点映射发货任务
        /// </summary>
        /// <param name="FHList"></param>
        /// <param name="Locationtable"></param>
        /// <param name="IsCompleted"></param>
        /// <param name="DeliveryId"></param>
        /// <returns></returns>
        private static bool P2PFHTask(List<DataRow> FHList,DataTable sLocationtable, DataTable eLocationtable, bool IsCompleted,string DeliveryId,bool CheckMappingLoc)
        {
            
            TypedData.T_SOM_OrderData _OrderData = new TypedData.T_SOM_OrderData();

            //获取BillId
            var so_BillId = GlobalContext.Resolve<ISource_BillHelper>().GetBillIdCustom("SO", Commons.StoreId, Commons.WarehouseId);

            //取起点和终点库位
            DataRow fromLocRow = null, toLocRow = null;
            fromLocRow = sLocationtable.AsEnumerable().Where(t => t["CID"].ConvertString() == FHList[0]["CID"].ConvertString()).FirstOrDefault();
            //起点先判断投料点是否有映射，有映射取映射库位，没有就走托盘。
            if (CheckMappingLoc && fromLocRow!=null && fromLocRow["WHAreaId"].ConvertString()=="C2")
            {
                var mappingLoc = GlobalContext.Resolve<ISource_SQLHelper>().ExecuteScalar(string.Format(mapLocationSql, FHList[0]["ProductLineCode"].ConvertString(), FHList[0]["PointCode"].ConvertString())).ToString();
                if (!string.IsNullOrWhiteSpace(mappingLoc))
                {
                    var locs = mappingLoc.Split(',');

                    var sMapLocationTable = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(string.Format(StartMapLocationSql, string.Join("','", locs))).Tables[0];
                    fromLocRow = sMapLocationTable.AsEnumerable().FirstOrDefault();
                    if (fromLocRow==null || fromLocRow["qty"].ConvertDecimal() <= 0)
                    {
                        //起始库位没有库存
                        var updatePT = $"UPDATE MES_PointTask SET Memo='{"映射起始库位没有库存"}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
                        GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePT);
                        NLogUtil.WriteError("映射起始库位没有库存");
                        return false;
                    }
                }
                else
                {
                    //映射库位为空的不发货。
                    var updatePT = $"UPDATE MES_PointTask SET Memo='{"映射起始库位为空"}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
                    GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePT);
                    NLogUtil.WriteError("映射起始库位为空");
                    return false;
                    //fromLocRow = sLocationtable.AsEnumerable().Where(t => t["CID"].ConvertString() == FHList[0]["CID"].ConvertString()).FirstOrDefault();
                    //验证库存是否>0
                }
            }
            else
            {
                fromLocRow = sLocationtable.AsEnumerable().Where(t => t["CID"].ConvertString() == FHList[0]["CID"].ConvertString()).FirstOrDefault();
            }

            toLocRow = eLocationtable.AsEnumerable().Where(t => t["ProductLineCode"].ConvertString() == FHList[0]["ProductLineCode"].ConvertString() && t["PointCode"].ConvertString() == FHList[0]["PointCode"].ConvertString()).FirstOrDefault();
            if (fromLocRow == null || toLocRow == null)
            {
                //移库起始点库存不足
                var updatePT = $"UPDATE MES_PointTask SET Memo='{"起始点或终点库位为空"}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
                GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePT);
                NLogUtil.WriteError("起始点或终点库位为空");
                IsCompleted = false;
                return IsCompleted;
            }
            //验证库存是否>0
            DataSet ds_Stock = Get_MoveStockList(fromLocRow["locationId"].ConvertString(), "", "", "", 0);
            DataTable dt = ds_Stock.Tables[0];
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    var som_OrderDetail_Row = _OrderData.SOM_OrderDetail.NewSOM_OrderDetailRow();
                    som_OrderDetail_Row.OrderLineNo = "1";
                    som_OrderDetail_Row.BillId = so_BillId;
                    som_OrderDetail_Row.StorerId = Commons.StoreId;
                    som_OrderDetail_Row.SkuId = item["SkuId"].ConvertString();
                    som_OrderDetail_Row.SkuStatus = "AVL";
                    som_OrderDetail_Row.Qty = item["QtyValid"].ConvertDecimal();
                    som_OrderDetail_Row.Status = "12";
                    som_OrderDetail_Row.PackageCode = item["PackageCode"].ConvertString();
                    som_OrderDetail_Row.PackageQty = item["PackageQty"].ConvertDecimal();
                    som_OrderDetail_Row.Version = 0;
                    som_OrderDetail_Row.CreateBy = "System";
                    som_OrderDetail_Row.CreateDate = DateTime.Now;
                    _OrderData.SOM_OrderDetail.AddSOM_OrderDetailRow(som_OrderDetail_Row);
                    var trm_OrderTaskDetail = _OrderData.TRM_OrderTaskDetail.NewTRM_OrderTaskDetailRow();
                    trm_OrderTaskDetail.OrderLineNo = som_OrderDetail_Row.OrderLineNo;
                    trm_OrderTaskDetail.OrderId = so_BillId;
                    trm_OrderTaskDetail.StorerId = Commons.StoreId;
                    trm_OrderTaskDetail.WarehouseId = Commons.WarehouseId;
                    trm_OrderTaskDetail.AreaId = fromLocRow["WHAreaId"].ConvertString();
                    trm_OrderTaskDetail.WHAreaId = toLocRow["WHAreaId"].ConvertString();
                    trm_OrderTaskDetail.CID = item["CID"].ConvertString();
                    trm_OrderTaskDetail.FromLoc = fromLocRow["LocationId"].ConvertString();
                    trm_OrderTaskDetail.ToLoc = toLocRow["LocationId"].ConvertString();
                    trm_OrderTaskDetail.BatchNo = item["BatchNo"].ConvertString();//取库存表的批次号
                    trm_OrderTaskDetail.SkuId = item["SkuId"].ConvertString();
                    trm_OrderTaskDetail.SkuStatus = "AVL";
                    trm_OrderTaskDetail.BoxCode = item["BoxCode"].ConvertString();
                    trm_OrderTaskDetail.QtyScan = item["QtyValid"].ConvertDecimal();
                    trm_OrderTaskDetail.Version = 0;
                    trm_OrderTaskDetail.CreateBy = "System";
                    trm_OrderTaskDetail.CreateDate = DateTime.Now;
                    _OrderData.TRM_OrderTaskDetail.AddTRM_OrderTaskDetailRow(trm_OrderTaskDetail);
                }
                var goodsCount = (from t in _OrderData.SOM_OrderDetail.AsEnumerable()
                                  group t by new { t1 = t.Field<string>("SkuId") } into m
                                  select new
                                  {
                                      SkuId = m.Key.t1
                                  }).ToList().Count;
                var skuCount = _OrderData.SOM_OrderDetail.AsEnumerable().Sum(t => t.Qty.ConvertDecimal().ConvertInt32());

                var som_Order_Row = _OrderData.SOM_Order.NewSOM_OrderRow();
                som_Order_Row.BillId = so_BillId;
                som_Order_Row.StorerId = Commons.StoreId;
                som_Order_Row.SyncBillId = DeliveryId;
                som_Order_Row.SOPType = "B2B";
                som_Order_Row.Status = "12";
                som_Order_Row.OrderSource = "MES";
                som_Order_Row.WarehouseId = Commons.WarehouseId;
                som_Order_Row.OrderType = "M2P-Out";//出库订单类型 巡线出库M2P-Out
                som_Order_Row.BusinessType = "M2P-OUT";//出库业务类型 巡线出库M2P-Out
                som_Order_Row.OrigSystem = "MES";
                som_Order_Row.Version = 0;
                som_Order_Row.CreateBy = "System";
                som_Order_Row.CreateDate = DateTime.Now;
                _OrderData.SOM_Order.AddSOM_OrderRow(som_Order_Row);
                var som_SyncOrder_Row = _OrderData.SOM_SyncOrder.NewSOM_SyncOrderRow();
                som_SyncOrder_Row.BillId = so_BillId;
                som_SyncOrder_Row.SyncBillId = DeliveryId;
                som_SyncOrder_Row.WarehouseId = Commons.WarehouseId;
                som_SyncOrder_Row.StorerId = Commons.StoreId;
                som_SyncOrder_Row.GoodsCount = goodsCount;
                som_SyncOrder_Row.SkuCount = skuCount;
                som_SyncOrder_Row.Ex7 = "1";
                som_SyncOrder_Row.Priority = FHList[0]["DockingOrder"].ConvertInt32();
                _OrderData.SOM_SyncOrder.AddSOM_SyncOrderRow(som_SyncOrder_Row);
            }
            else
            {
                //移库起始点库存不足
                var updatePT = $"UPDATE MES_PointTask SET Memo='{"移库起始点库存不足"}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
                GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePT);
                NLogUtil.WriteError("移库起始点库存不足");
                IsCompleted = false;
                return false;
            }

            var updatePTDetail = $"UPDATE MES_PointTask SET Memo='{"保存投料发货任务失败"}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
            try
            {
                var res = GlobalContext.Resolve<ISource_SQLHelper>().Save(_OrderData);
            }
            catch (Exception)
            {
                IsCompleted = false;
                GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePTDetail);
                NLogUtil.WriteError("保存投料发货任务失败");
                return IsCompleted;
            }
            //保存任务成功后回写投料明显表WMSOrderId
            updatePTDetail = $"UPDATE MES_PointTask SET WMSOrderId='{so_BillId}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10' " +
                $"update MES_PointTaskDetail set WMSOrderId='{so_BillId}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' where FlowNo={FHList[0]["FlowNo"].ConvertInt32()} and DeliveryId='{DeliveryId}' And Status='10'";
            GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePTDetail);
            
            return IsCompleted;
        }
        /// <summary>
        /// 多点巡线发货任务
        /// </summary>
        /// <param name="FHList"></param>
        /// <param name="sLocationtable"></param>
        /// <param name="eLocationtable"></param>
        /// <param name="IsCompleted"></param>
        /// <param name="DeliveryId"></param>
        /// <param name="CheckMappingLoc"></param>
        /// <param name="_OrderData"></param>
        /// <returns></returns>
        private static bool FHTask(List<DataRow> FHList, DataTable sLocationtable, DataTable eLocationtable, bool IsCompleted, string DeliveryId, bool CheckMappingLoc, TypedData.T_MES_M2PTaskData _OrderData = null)
        {
            //if (_OrderData == null)
            //{
            //    _OrderData = new TypedData.T_SOM_OrderData();
            //}


            //获取BillId
            var so_BillId = GlobalContext.Resolve<ISource_BillHelper>().GetBillIdCustom("SO", Commons.StoreId, Commons.WarehouseId);

            //取起点和终点库位
            DataRow fromLocRow = null, toLocRow = null;
            //起点先判断投料点是否有映射，有映射取映射库位，没有就走托盘。
            if (CheckMappingLoc)
            {
                var mappingLoc = GlobalContext.Resolve<ISource_SQLHelper>().ExecuteScalar(string.Format(mapLocationSql, FHList[0]["ProductLineCode"].ConvertString(), FHList[0]["PointCode"].ConvertString())).ToString();
                if (!string.IsNullOrWhiteSpace(mappingLoc))
                {
                    var sMapLocationTable = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(string.Format(StartMapLocationSql, mappingLoc)).Tables[0];
                    fromLocRow = sMapLocationTable.AsEnumerable().FirstOrDefault();
                    if (fromLocRow["qty"].ConvertDecimal() <= 0)
                    {
                        //起始库位没有库存
                        var updatePT = $"UPDATE MES_PointTask SET Memo='{"映射起始库位没有库存"}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
                        GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePT);
                        NLogUtil.WriteError("映射起始库位没有库存");
                        return false;
                    }
                }
                else
                {
                    //映射库位为空的不发货。
                    var updatePT = $"UPDATE MES_PointTask SET Memo='{"映射起始库位为空"}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
                    GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePT);
                    NLogUtil.WriteError("映射起始库位为空");
                    return false;
                    //fromLocRow = sLocationtable.AsEnumerable().Where(t => t["CID"].ConvertString() == FHList[0]["CID"].ConvertString()).FirstOrDefault();
                    //验证库存是否>0
                }
            }
            else
            {
                fromLocRow = sLocationtable.AsEnumerable().Where(t => t["CID"].ConvertString() == FHList[0]["CID"].ConvertString()).FirstOrDefault();
            }

            toLocRow = eLocationtable.AsEnumerable().Where(t => t["ProductLineCode"].ConvertString() == FHList[0]["ProductLineCode"].ConvertString() && t["PointCode"].ConvertString() == FHList[0]["PointCode"].ConvertString()).FirstOrDefault();
            if (fromLocRow == null || toLocRow == null)
            {
                //移库起始点库存不足
                var updatePT = $"UPDATE MES_PointTask SET Memo='{"起始点或终点库位为空"}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
                GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePT);
                NLogUtil.WriteError("起始点或终点库位为空");
                IsCompleted = false;
                return IsCompleted;
            }
            //验证库存是否>0
            DataSet ds_Stock = Get_MoveStockList(fromLocRow["locationId"].ConvertString(), "", "", "", 0);
            DataTable dt = ds_Stock.Tables[0];
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    var som_OrderDetail_Row = _OrderData.SOM_OrderDetail.NewSOM_OrderDetailRow();
                    som_OrderDetail_Row.OrderLineNo = "1";
                    som_OrderDetail_Row.BillId = so_BillId;
                    som_OrderDetail_Row.StorerId = Commons.StoreId;
                    som_OrderDetail_Row.SkuId = item["SkuId"].ConvertString();
                    som_OrderDetail_Row.SkuStatus = "AVL";
                    som_OrderDetail_Row.Qty = item["QtyValid"].ConvertDecimal();
                    som_OrderDetail_Row.Status = "12";
                    som_OrderDetail_Row.PackageCode = item["PackageCode"].ConvertString();
                    som_OrderDetail_Row.PackageQty = item["PackageQty"].ConvertDecimal();
                    som_OrderDetail_Row.Version = 0;
                    som_OrderDetail_Row.CreateBy = "System";
                    som_OrderDetail_Row.CreateDate = DateTime.Now;
                    _OrderData.SOM_OrderDetail.AddSOM_OrderDetailRow(som_OrderDetail_Row);
                    var trm_OrderTaskDetail = _OrderData.TRM_OrderTaskDetail.NewTRM_OrderTaskDetailRow();
                    trm_OrderTaskDetail.OrderLineNo = som_OrderDetail_Row.OrderLineNo;
                    trm_OrderTaskDetail.OrderId = so_BillId;
                    trm_OrderTaskDetail.StorerId = Commons.StoreId;
                    trm_OrderTaskDetail.WarehouseId = Commons.WarehouseId;
                    trm_OrderTaskDetail.AreaId = fromLocRow["WHAreaId"].ConvertString();
                    trm_OrderTaskDetail.WHAreaId = toLocRow["WHAreaId"].ConvertString();
                    trm_OrderTaskDetail.CID = item["CID"].ConvertString();
                    trm_OrderTaskDetail.FromLoc = fromLocRow["LocationId"].ConvertString();
                    trm_OrderTaskDetail.ToLoc = toLocRow["LocationId"].ConvertString();
                    trm_OrderTaskDetail.BatchNo = item["BatchNo"].ConvertString();//取库存表的批次号
                    trm_OrderTaskDetail.SkuId = item["SkuId"].ConvertString();
                    trm_OrderTaskDetail.SkuStatus = "AVL";
                    trm_OrderTaskDetail.BoxCode = item["BoxCode"].ConvertString();
                    trm_OrderTaskDetail.QtyScan = item["QtyValid"].ConvertDecimal();
                    trm_OrderTaskDetail.Version = 0;
                    trm_OrderTaskDetail.CreateBy = "System";
                    trm_OrderTaskDetail.CreateDate = DateTime.Now;
                    _OrderData.TRM_OrderTaskDetail.AddTRM_OrderTaskDetailRow(trm_OrderTaskDetail);
                }
                var goodsCount = (from t in _OrderData.SOM_OrderDetail.AsEnumerable()
                                  group t by new { t1 = t.Field<string>("SkuId") } into m
                                  select new
                                  {
                                      SkuId = m.Key.t1
                                  }).ToList().Count;
                var skuCount = _OrderData.SOM_OrderDetail.AsEnumerable().Sum(t => t.Qty.ConvertDecimal().ConvertInt32());

                var som_Order_Row = _OrderData.SOM_Order.NewSOM_OrderRow();
                som_Order_Row.BillId = so_BillId;
                som_Order_Row.StorerId = Commons.StoreId;
                som_Order_Row.SyncBillId = DeliveryId;
                som_Order_Row.SOPType = "B2B";
                som_Order_Row.Status = "12";
                som_Order_Row.OrderSource = "MES";
                som_Order_Row.WarehouseId = Commons.WarehouseId;
                som_Order_Row.OrderType = "M2P-Out";//出库订单类型 巡线出库M2P-Out
                som_Order_Row.BusinessType = "M2P-OUT";//出库业务类型 巡线出库M2P-Out
                som_Order_Row.OrigSystem = "MES";
                som_Order_Row.Version = 0;
                som_Order_Row.CreateBy = "System";
                som_Order_Row.CreateDate = DateTime.Now;
                _OrderData.SOM_Order.AddSOM_OrderRow(som_Order_Row);
                var som_SyncOrder_Row = _OrderData.SOM_SyncOrder.NewSOM_SyncOrderRow();
                som_SyncOrder_Row.BillId = so_BillId;
                som_SyncOrder_Row.SyncBillId = DeliveryId;
                som_SyncOrder_Row.WarehouseId = Commons.WarehouseId;
                som_SyncOrder_Row.StorerId = Commons.StoreId;
                som_SyncOrder_Row.GoodsCount = goodsCount;
                som_SyncOrder_Row.SkuCount = skuCount;
                som_SyncOrder_Row.Ex7 = "1";
                som_SyncOrder_Row.Priority = toLocRow["PutAwayOrder"].ConvertInt32();
                _OrderData.SOM_SyncOrder.AddSOM_SyncOrderRow(som_SyncOrder_Row);
            }
            else
            {
                //移库起始点库存不足
                var updatePT = $"UPDATE MES_PointTask SET Memo='{"移库起始点库存不足"}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
                GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePT);
                NLogUtil.WriteError("移库起始点库存不足");
                IsCompleted = false;
                return false;
            }

            var updatePTDetail = $"UPDATE MES_PointTask SET Memo='{"保存投料发货任务失败"}',ModifyBy='WCP2PTask',ModifyDate='{DateTime.Now}' WHERE DeliveryId='{DeliveryId}' And Status='10'";
            //try
            //{
            //    var res = GlobalContext.Resolve<ISource_SQLHelper>().Save(_OrderData);
            //}
            //catch (Exception)
            //{
            //    IsCompleted = false;
            //    GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePTDetail);
            //    NLogUtil.WriteError("保存投料发货任务失败");
            //    return IsCompleted;
            //}
            //保存任务成功后回写投料明显表WMSOrderId
            var taskRow = _OrderData.MES_PointTask.Where(t => t.DeliveryId == DeliveryId).FirstOrDefault();
            var taskDetailRow = _OrderData.MES_PointTaskDetail.Where(t => t.DeliveryId == DeliveryId && t.FlowNo== FHList[0]["FlowNo"].ConvertLong()).FirstOrDefault();

            taskRow.WMSOrderId = so_BillId;
            taskDetailRow.WMSOrderId = so_BillId;

            //updatePTDetail = $"UPDATE MES_PointTask SET WMSOrderId='{so_BillId}' WHERE DeliveryId='{DeliveryId}' update MES_PointTaskDetail set WMSOrderId='{so_BillId}' where FlowNo={FHList[0]["FlowNo"].ConvertInt32()} and DeliveryId='{DeliveryId}'";
            //GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(updatePTDetail);

            return IsCompleted;
        }
        /// <summary>
        /// 获取移库库存信息
        /// </summary>
        public static DataSet Get_MoveStockList(string locationId, string skuId="", string batchNo = "", string boxCode = "", int source = 0)
        {
            string sql = $"SELECT A.LocationId,A.SkuId,B.SkuName,A.BatchNo,A.CID,A.BoxCode,A.ProductDate,A.ExpiryDate,A.QtyValid,A.PackageCode,A.PackageQty,A.WarehouseId,A.SkuStatus,D.WHAreaId,A.StorerId,A.WarehouseId FROM INV_BAL A JOIN COM_SKU B ON A.SkuId = B.SkuId AND A.StorerId = B.StorerId JOIN COM_Location D ON A.LocationId = D.LocationId JOIN COM_WHArea E ON D.WHAreaId = E.WHAreaId WHERE  A.QtyValid >0  AND  A.LocationId = '{locationId}'";
            if (source == 0)
            {
                sql += " AND E.WHAreaType NOT IN ('EXCHANGE','TMP_STORAGE') ";
            }
            if (batchNo != "")
            {
                sql += $" AND ISNULL(A.batchNo,'')='{batchNo}' ";
            }
            if (boxCode != "")
            {
                sql += $" AND ISNULL(A.BoxCode,'')='{boxCode}' ";
            }
            return GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql);
        }
    }
}
