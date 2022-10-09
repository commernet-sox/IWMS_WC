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

namespace iWMS.WC2API.Handlers
{
	/// <summary>
	/// 产线MES 过点信息
	/// </summary>
	public class PullPointEventHandler : ISyncListEventHandler<PullPointRequest, Outcome>
    {
        public Outcome Handle(IEnumerable<PullPointRequest> data)
        {
			if (data == null || !data.Any())
			{
				return new Outcome(400, "数据列表为空");
			}
			if (data.Any(t => t.TaskId == null || t.TaskId == ""))
			{
				return new Outcome(400, "任务号不能为空");
			}
			if (data.Any(t => t.ProductCode == null || t.ProductCode == ""))
			{
				return new Outcome(400, "发动机序列号不能为空");
			}
			if (data.Any(t => t.ProductLineCode == null || t.ProductLineCode == ""))
			{
				return new Outcome(400, "产线编码不能为空");
			}
			if (data.Any(t => t.OperationCode == null || t.OperationCode == ""))
			{
				return new Outcome(400, "工位编码不能为空");
			}
			var typeData = new T_MES_PullPointData();
			var sql = $"select * from MES_PullPoint where TaskId in ('{string.Join("','", data.Select(t => t.TaskId))}')";
			var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.MES_PullPoint.TableName);
			typeData.Merge(typeData.MES_PullPoint.TableName, ds);
			foreach (var item in data)
			{
				//如果工位编码有多个，金燕需要分开保存
				var stationCodes = item.OperationCode.Split(',');
				if (stationCodes.Count() > 0)
				{
					foreach (var it in stationCodes)
					{
						var row = typeData.MES_PullPoint.Where(t => t.TaskId ==item.TaskId && t.StationCode==it).FirstOrDefault();
						if (row != null)
						{
							row.Version = row["Version"].ConvertInt32() + 1;
							row.ModifyBy = "WC2API";
							row.ModifyDate = GlobalContext.ServerTime;
							//row.StationCode = it;

							row.ProductCode = item.ProductCode;
							row.ProductLineCode = item.ProductLineCode;
							row.StationTime = item.StationTime;
							row.SendTime = item.SendTime;
							row.Memo = "";
						}
						else
						{
							row = typeData.MES_PullPoint.NewMES_PullPointRow();
							row.Version = 1;
							row.CreateBy = "WC2API";
							row.CreateDate = GlobalContext.ServerTime;
							row.TaskId = item.TaskId;
							row.StationCode = it;
							row.ProductCode = item.ProductCode;
							row.ProductLineCode = item.ProductLineCode;
							row.StationTime = item.StationTime;
							row.SendTime = item.SendTime;
							row.Memo = "";
							typeData.MES_PullPoint.AddMES_PullPointRow(row);
						}
					}
					
				}
				else
				{
					var row = typeData.MES_PullPoint.FindByTaskId(item.TaskId);
					if (row != null)
					{
						row.Version = row["Version"].ConvertInt32() + 1;
						row.ModifyBy = "WC2API";
						row.ModifyDate = GlobalContext.ServerTime;
						row.ProductCode = item.ProductCode;
						row.ProductLineCode = item.ProductLineCode;
						row.StationCode = item.OperationCode;
						row.StationTime = item.StationTime;
						row.SendTime = item.SendTime;
						row.Memo = "";

					}
					else
					{

						row = typeData.MES_PullPoint.NewMES_PullPointRow();
						row.Version = 1;
						row.CreateBy = "WC2API";
						row.CreateDate = GlobalContext.ServerTime;
						row.TaskId = item.TaskId;
						row.ProductCode = item.ProductCode;
						row.ProductLineCode = item.ProductLineCode;
						row.StationCode = item.OperationCode;
						row.StationTime = item.StationTime;
						row.SendTime = item.SendTime;
						row.Memo = "";
						typeData.MES_PullPoint.AddMES_PullPointRow(row);
					}
				}
				
			}
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
		}
    }
}