using Common.ISource;
using FWCore;
using iWMS.Core;
using iWMS.TypedData;
using iWMS.WC2API.Core;
using iWMS.WC2API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using iWMS.WebApplication;

namespace iWMS.WC2API.Handlers
{
	/// <summary>
	/// 产线MES 退空任务下发
	/// </summary>
	public class EmptyPalletBackEventHandler : ISyncListEventHandler<EmptyPalletBackRequest, Outcome>
    {
        public Outcome Handle(IEnumerable<EmptyPalletBackRequest> data)
        {

			if (data == null || !data.Any())
			{
				return new Outcome(400, "数据列表为空");
			}
			if (data.Any(t => t.TaskId == null || t.TaskId == ""))
			{
				return new Outcome(400, "任务号不能为空");
			}
			if (data.Any(t => t.ProductLineCode == null || t.ProductLineCode == ""))
			{
				return new Outcome(400, "产线编码不能为空");
			}
			if (data.Any(t => t.OperationCode == null || t.OperationCode == ""))
			{
				return new Outcome(400, "工位编码不能为空");
			}
			if (data.Any(t => t.LocationId == null || t.LocationId == ""))
			{
				return new Outcome(400, "取空托位置不能为空");
			}
			if (data.Any(t => t.PalletId == null || t.PalletId == ""))
			{
				return new Outcome(400, "空托盘号不能为空");
			}
			string existSql = " SELECT COUNT(1) FROM dbo.TRM_TaskOther WHERE TaskId IN ('{0}')";
			existSql = string.Format(existSql,string.Join("','", data.Select(t=>t.TaskId)));
			var count = GlobalContext.Resolve<ISource_SQLHelper>().ExecuteScalar(existSql).ConvertInt32();
			if (count > 0)
			{
				return new Outcome(400, "退空任务TaskId不能重复！");
			}
			var typeData = new T_TRM_TaskOtherData();

            string toLocSql = @"select ParmValue from SYS_Parameter where ParmId='POTHER003'";
			var toLoc = GlobalContext.Resolve<ISource_SQLHelper>().ExecuteScalar(toLocSql).ConvertString();
			if (toLoc == null || toLoc == "")
			{
				return new Outcome(400, "配置的退空目标库位为空，请稍后再试！");
			}
			string mapLoc = @"SELECT EmptyMappingLoc FROM MES_Point WHERE ProductLineCode='{0}' AND PointCode='{1}'";
			/// <summary>
			/// 判断映射点库位是否有库存
			/// </summary>
			const string StartMapLocationSql = @"
			SELECT a.LocationId,a.WHAreaId,a.CID,a.Property1 AS ProductLineCode,a.Property2 AS PointCode,cast(0 as decimal(18,4)) qty,a.PutAwayOrder into #tmp FROM COM_Location a where a.LocationId in ('{0}') and a.Status='1' and a.WHAreaId='C2' 

			update t set t.qty=(select sum(qty) from INV_BAL where LocationId=t.LocationId)
			from #tmp t 

			select * from #tmp where qty>0 order by PutAwayOrder asc";
			//保存为其他任务
			foreach (var item in data)
			{
				var fromLocRow = item.LocationId;
				//获取存储区和产线区的库位
				string LocationSql = $"SELECT LocationId,CID,WHAreaId,Property1 AS ProductLineCode,Property2 AS PointCode FROM COM_Location where Status=1 and LocationId='{fromLocRow}' ";
				var Locationtable = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(LocationSql).Tables[0];
				var obj = Locationtable.AsEnumerable().FirstOrDefault();
				if (obj == null)
				{
					return new Outcome(400, "找不到取空托位置，请稍后再试！");
				}
				var whAreaId = obj["WHAreaId"].ConvertString();
				var pointCode = obj["PointCode"].ConvertString();
				var mappingLoc = GlobalContext.Resolve<ISource_SQLHelper>().ExecuteScalar(string.Format(mapLoc, item.ProductLineCode, pointCode)).ToString();
				//优先取映射的库位，否则取配置库位。
				var toLocRow = "";
				if (!string.IsNullOrWhiteSpace(mappingLoc))
				{
					var locs = mappingLoc.Split(',');
					var sMapLocationTable = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(string.Format(StartMapLocationSql, string.Join("','",locs))).Tables[0];
					var maploc = sMapLocationTable.AsEnumerable().FirstOrDefault();
					if (maploc != null)
					{
						toLocRow = maploc["LocationId"].ConvertString();
					}
				}
				else
				{
					toLocRow = toLoc;
				}
				toLocRow = toLocRow==""? toLoc: toLocRow;
				var taskIds = GlobalContext.Resolve<ISource_SQLHelper>().ExecuteScalar($"DECLARE @Id VARCHAR(30) exec SP_GEN_TASKID 'TO',{Commons.StoreId},{Commons.WarehouseId},@Id OUTPUT SELECT @Id").ConvertString();
				var trm_TaskOther_Row = typeData.TRM_TaskOther.NewTRM_TaskOtherRow();
				trm_TaskOther_Row.TaskId = taskIds;
				trm_TaskOther_Row.TaskType = "50";
				trm_TaskOther_Row.Status = "10";
				trm_TaskOther_Row.Priority = 1;
				trm_TaskOther_Row.WHAreaId = whAreaId;
				trm_TaskOther_Row.FromLoc = fromLocRow;
				trm_TaskOther_Row.ToLoc = toLocRow;
				trm_TaskOther_Row.CID = item.PalletId;
				trm_TaskOther_Row.IsAgvCompleted = 0;
				trm_TaskOther_Row.Ex9 = item.TaskId;
				//trm_TaskOther_Row.Ex10 = item.TaskId;
				trm_TaskOther_Row.Version = 0;
				trm_TaskOther_Row.CreateBy = "WC2API";
				trm_TaskOther_Row.CreateDate = DateTime.Now;
				typeData.TRM_TaskOther.AddTRM_TaskOtherRow(trm_TaskOther_Row);
			}
			
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
		}
    }
}