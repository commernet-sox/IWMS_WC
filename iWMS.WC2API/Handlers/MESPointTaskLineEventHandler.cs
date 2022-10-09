using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FWCore;
using iWMS.WC2API.Models;
using iWMS.TypedData;
using Common.DBCore;
using System.Data;
using iWMS.Core;
using Common.ISource;
using iWMS.WebApplication;

namespace iWMS.WC2API.Handlers
{
	/// <summary>
	/// 巡线任务
	/// </summary>
	public class MESPointTaskLineEventHandler : ISyncListEventHandler<MESPointTaskLineRequest, Outcome>
    {
        public Outcome Handle(IEnumerable<MESPointTaskLineRequest> data)
        {
			if (data == null || !data.Any())
			{
				return new Outcome(400, "数据列表为空");
			}
			if (data.Any(t => t.DeliverySerial == null || t.DeliverySerial == ""))
			{
				return new Outcome(400, "投料单号不能为空");
			}
			if (data.Any(t => t.Status == null || t.Status == ""))
			{
				return new Outcome(400, "状态不能为空");
			}
			if (data.Any(t => t.Appliance == null || t.Appliance == ""))
			{
				return new Outcome(400, "托盘码(器具码)不能为空");
			}
			if (data.Any(t => t.PullNumber == null || t.PullNumber == ""))
			{
				return new Outcome(400, "拉动条码不能为空");
			}
			if (data.Any(t => t.MaterialCode == null || t.MaterialCode == ""))
			{
				return new Outcome(400, "物料编码不能为空");
			}
			if (data.Any(t => t.MaterialName == null || t.MaterialName == ""))
			{
				return new Outcome(400, "物料名称不能为空");
			}
			if (data.Any(t => t.NeedNum == null || t.NeedNum == ""))
			{
				return new Outcome(400, "数量不能为空");
			}
			if (data.Any(t => t.CarGroupCode == null || t.CarGroupCode == ""))
			{
				return new Outcome(400, "装车组编码不能为空");
			}
			if (data.Any(t => t.CarGroupName == null || t.CarGroupName == ""))
			{
				return new Outcome(400, "装车组名称不能为空");
			}
			if (data.Any(t => t.ProductLineCode == null || t.ProductLineCode == ""))
			{
				return new Outcome(400, "产线编码不能为空");
			}
			if (data.Any(t => t.ProductLineName == null || t.ProductLineName == ""))
			{
				return new Outcome(400, "产线名称不能为空");
			}
			if (data.Any(t => t.PlanDate == null))
			{
				return new Outcome(400, "计划日期不能为空");
			}
			if (data.Any(t => t.FeedingMode == null || t.FeedingMode == ""))
			{
				return new Outcome(400, "投料方式不能为空");
			}
			if (data.Any(t => t.StationCode == null || t.StationCode == ""))
			{
				return new Outcome(400, "工位编码不能为空");
			}
			if (data.Any(t => t.StationName == null || t.StationName == ""))
			{
				return new Outcome(400, "工位名称不能为空");
			}
			if (data.Any(t => t.WaveNum == null || t.WaveNum == ""))
			{
				return new Outcome(400, "波次不能为空");
			}
			if (data.Any(t => t.BatchNum == null || t.BatchNum == ""))
			{
				return new Outcome(400, "批次不能为空");
			}
			if (data.Any(t => t.FeedingPointCode == null || t.FeedingPointCode == ""))
			{
				return new Outcome(400, "投料点编码不能为空");
			}
			if (data.Any(t => t.FeedingPointName == null || t.FeedingPointName == ""))
			{
				return new Outcome(400, "投料点名称不能为空");
			}
			if (data.Any(t => t.UtensilTypeCode == null || t.UtensilTypeCode == ""))
			{
				return new Outcome(400, "器具类型编码不能为空");
			}
			if (data.Any(t => t.LineWaveNum == null || t.LineWaveNum == ""))
			{
				return new Outcome(400, "产线波次不能为空");
			}
			if (data.Any(t => t.PackSort == null))
			{
				return new Outcome(400, "包装顺序不能为空");
			}
			//if (data.Any(t => t.DockingOrder == null || t.DockingOrder == ""))
			//{
			//	return new Outcome(400, "停靠顺序不能为空");
			//}
			//if (data.Any(t => t.StopPointName == null || t.StopPointName == ""))
			//{
			//	return new Outcome(400, "停靠点不能为空");
			//}

			var typeData = new T_MES_PointTaskData();
			var sql = $"select * from MES_PointTask where DeliveryId in ('{string.Join("','", data.Select(t => t.DeliverySerial))}')";
			var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.MES_PointTask.TableName);
			typeData.Merge(typeData.MES_PointTask.TableName, ds);

			//不允许更新，以防止其他程序操作后造成数据异常。
			List<string> error = new List<string>();
			if (typeData.MES_PointTask.Count > 0)
			{
				error = typeData.MES_PointTask.Select(t => t.DeliveryId).ToList();
				return new Outcome(400, $"{string.Join(",", error)} 投料单已存在，请重新输入！");
			}

			var gList = data.GroupBy(t => t.DeliverySerial).ToList();
			foreach (var group in gList)
			{
				var deliveryId = group.Key;
				var Detail = group.ToList();
				var row = typeData.MES_PointTask.NewMES_PointTaskRow();
				row.Version = 1;
				row.CreateBy = "WC2API";
				row.CreateDate = GlobalContext.ServerTime;
				row.DeliveryId = deliveryId;
				row.DeliveryStatus = Detail[0].Status;
				row.PlanDate = Detail[0].PlanDate ?? DateTime.Now;
				row.FeedingMode = Detail[0].FeedingMode;
				row.CIDType = Detail[0].UtensilTypeCode;
				row.OrigSystem = "LES";
				if (row.FeedingMode == "AGV点对点")
				{
					row.DeliveryType = "P2P";
				}
				else if (row.FeedingMode == "AGV巡线")
				{
					row.DeliveryType = "M2P";
				}
				else
				{
					return new Outcome(400, "投料方式不正确,请重试！");
				}
				row.Status = "10";
				typeData.MES_PointTask.AddMES_PointTaskRow(row);

				foreach (var item in Detail)
				{
					var rowDtl = typeData.MES_PointTaskDetail.NewMES_PointTaskDetailRow();
					rowDtl.Version = 1;
					rowDtl.CreateBy = "WC2API";
					rowDtl.CreateDate = GlobalContext.ServerTime;
					rowDtl.DeliveryId = row.DeliveryId;

					rowDtl.Status = "10";
					rowDtl.CID = item.Appliance;
					rowDtl.PullNumber = item.PullNumber;
					rowDtl.SkuId = item.MaterialCode;
					rowDtl.SkuName = item.MaterialName;
					rowDtl.Qty = item.NeedNum.ConvertDecimal();
					rowDtl.CarGroupCode = item.CarGroupCode;
					rowDtl.CarGroupName = item.CarGroupName;
					rowDtl.ProductLineCode = item.ProductLineCode;
					rowDtl.ProductLineName = item.ProductLineName;
					rowDtl.StationCode = item.StationCode;
					rowDtl.StationName = item.StationName;
					rowDtl.WaveNum = item.WaveNum;
					rowDtl.BatchNum = item.BatchNum;
					rowDtl.PointCode = item.FeedingPointCode;
					rowDtl.PointName = item.FeedingPointName;
					rowDtl.DockingOrder = item.DockingOrder.ConvertInt32();
					rowDtl.StopPointName = item.StopPointName;
					rowDtl.ProductLineNum = item.LineWaveNum;
					rowDtl.PackSort = item.PackSort??0;
					//器具类型编码
					typeData.MES_PointTaskDetail.AddMES_PointTaskDetailRow(rowDtl);
				}
			}
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
		}
    }
}