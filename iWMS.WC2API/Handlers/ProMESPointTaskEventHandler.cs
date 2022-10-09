using Common.ISource;
using FWCore;
using iWMS.APILog;
using iWMS.Core;
using iWMS.TypedData;
using iWMS.WC2API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.WebApplication;
using iWMS.WC2API.Core;
using System.Data;

namespace iWMS.WC2API.Handlers
{
	/// <summary>
	/// 产线MES 投料任务下发
	/// </summary>
	public class ProMESPointTaskEventHandler : ISyncListEventHandler<ProMESPointTaskRequest, Outcome>
    {
        public Outcome Handle(IEnumerable<ProMESPointTaskRequest> data)
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
				return new Outcome(400, "投料点位不能为空");
			}
			var origSystem = Commons.GetOrigSystem();
			var typeData = new T_MES_PointTaskInterfaceData();
			foreach (var item in data)
			{
				//先保存mes投料任务
				
				var sql = $"select * from MES_PointTask_Interface where TaskId in ('{string.Join("','", data.Select(t => t.TaskId))}')";
				var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.MES_PointTask_Interface.TableName);
				typeData.Merge(typeData.MES_PointTask_Interface.TableName, ds);

				//不允许更新，以防止其他程序操作后造成数据异常。
				List<string> error = new List<string>();
				if (typeData.MES_PointTask_Interface.Count > 0)
				{
					error = typeData.MES_PointTask_Interface.Select(t => t.TaskId).ToList();
					return new Outcome(400, $"{string.Join(",", error)} 任务号已存在，请重新输入！");
				}
				var row = typeData.MES_PointTask_Interface.FindByTaskId(item.TaskId);

				row = typeData.MES_PointTask_Interface.NewMES_PointTask_InterfaceRow();
				row.Version = 1;
				row.CreateBy = "WC2API";
				row.CreateDate = GlobalContext.ServerTime;
				row.TaskId = item.TaskId;
				row.ProductLineCode = item.ProductLineCode;
				row.OperationCode = item.OperationCode;
				row.PointCode = item.LocationId;
				row.IsCompleted = 0;
				row.Ex1 = origSystem;
				typeData.MES_PointTask_Interface.AddMES_PointTask_InterfaceRow(row);



				//定时任务来做
				//先找投料点看有没有映射库位，按库位顺序找投料任务
				//string mapLoc = @"SELECT MappingLoc FROM MES_Point WHERE ProductLineCode='{0}' AND PointCode='{1}'";
				//var mappingLoc = GlobalContext.Resolve<ISource_SQLHelper>().ExecuteScalar(string.Format(mapLoc, item.ProductLineCode, item.LocationId)).ToString();
				
				//if (!string.IsNullOrWhiteSpace(mappingLoc))
				//{
				//	var locs = mappingLoc.Split(',');
				//	foreach (var it in locs)
				//	{
				//		string LocationSql = $"SELECT LocationId,CID,WHAreaId,Property1 AS ProductLineCode,Property2 AS PointCode FROM COM_Location where Status=1 and LocationId='{it}'";
				//		var Locationtable = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(LocationSql).Tables[0];
				//		var obj = Locationtable.AsEnumerable().FirstOrDefault();
				//		if (obj == null)
				//		{
				//			continue;
				//		}
				//		else
				//		{
				//			var ProductLineCode = obj["ProductLineCode"].ConvertString();
				//			var pointCode = obj["PointCode"].ConvertString();
				//			var pointTaskDetailSql = $"SELECT TOP 1 A.DeliveryId FROM MES_PointTask A INNER JOIN MES_PointTaskDetail B ON A.DeliveryId=B.DeliveryId WHERE A.DeliveryType = 'P2P' AND A.Status = '20' AND B.ProductLineCode = '{ProductLineCode}' AND B.PointCode = '{pointCode}' ORDER BY A.ModifyDate ASC";
				//			var dsPoint = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(pointTaskDetailSql);
				//			if (dsPoint != null && dsPoint.Tables[0].Rows.Count > 0)
				//			{
				//				var dt = dsPoint.Tables[0];

				//				var updateSql = @"UPDATE MES_PointTask SET Status='30',MESTaskId='{1}',OrigSystem='{2}',ModifyBy='System',ModifyDate=GETDATE(),Version=Version+1 WHERE DeliveryId='{0}' And Status=='20';
				//						  UPDATE MES_PointTaskDetail SET Status='30',ModifyBy='System',ModifyDate=GETDATE(),Version=Version+1 WHERE DeliveryId='{0}' And Status=='20'";
				//				try
				//				{
				//					GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(string.Format(updateSql, dt.Rows[0]["DeliveryId"].ConvertString(), item.TaskId, origSystem));

				//				}
				//				catch (Exception ex)
				//				{
				//					NLogUtil.WriteError(ex.ToString());
				//				}
				//			}
				//			else
				//			{
				//				continue;
				//				//return new Outcome(400, "参数错误，未找到投料任务");
				//			}
				//		}
				//	}
				//}
				//else
				//{
				//	var pointTaskDetailSql = $"SELECT TOP 1 A.DeliveryId FROM MES_PointTask A INNER JOIN MES_PointTaskDetail B ON A.DeliveryId=B.DeliveryId WHERE A.DeliveryType = 'P2P' AND A.Status = '20' AND B.ProductLineCode = '{item.ProductLineCode}' AND B.PointCode = '{item.LocationId}' ORDER BY A.ModifyDate ASC";
				//	var dsPoint = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(pointTaskDetailSql);
				//	if (dsPoint != null && dsPoint.Tables[0].Rows.Count > 0)
				//	{
				//		var dt = dsPoint.Tables[0];

				//		var updateSql = @"UPDATE MES_PointTask SET Status='30',MESTaskId='{1}',OrigSystem='{2}',ModifyBy='System',ModifyDate=GETDATE(),Version=Version+1 WHERE DeliveryId='{0}' And Status=='20';
				//						  UPDATE MES_PointTaskDetail SET Status='30',ModifyBy='System',ModifyDate=GETDATE(),Version=Version+1 WHERE DeliveryId='{0}' And Status=='20'";
				//		try
				//		{
				//			GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(string.Format(updateSql, dt.Rows[0]["DeliveryId"].ConvertString(), item.TaskId, origSystem));

				//		}
				//		catch (Exception ex)
				//		{
				//			NLogUtil.WriteError(ex.ToString());
				//		}
				//	}
				//	else
				//	{
				//		return new Outcome(400, "参数错误，未找到投料任务");
				//	}
				//}
				
			}
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
		}	
    }
}