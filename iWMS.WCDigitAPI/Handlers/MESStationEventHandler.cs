using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FWCore;
using iWMS.TypedData;
using Common.DBCore;
using System.Data;
using iWMS.Core;
using Common.ISource;
using iWMS.WCDigitAPI.Models;

namespace iWMS.WCDigitAPI.Handlers
{
	/// <summary>
	/// 工位管理
	/// </summary>
	public class MESStationEventHandler : ISyncListEventHandler<MESStationRequest, Outcome>
    {
        public Outcome Handle(IEnumerable<MESStationRequest> data)
        {
			if (data == null || !data.Any())
			{
				return new Outcome(500, "数据列表为空");
			}
			if (data.Any(t => t.StationName == null || t.StationName == ""))
			{
				return new Outcome(500, "工位名称不能为空");
			}
			if (data.Any(t => t.StationCode == null || t.StationCode == ""))
			{
				return new Outcome(500, "工位编码不能为空");
			}
			if (data.Any(t => t.StationRange == null || t.StationRange == ""))
			{
				return new Outcome(500, "工段不能为空");
			}
			if (data.Any(t => t.ProductlineCode == null))
			{
				return new Outcome(500, "所属产线编码不能为空");
			}
			if (data.Any(t => t.IntervalTime == null ))
			{
				return new Outcome(500, "工位间隔时间不能为空");
			}
			if (data.Any(t => t.PassPullStationCode == null || t.PassPullStationCode == ""))
			{
				return new Outcome(500, "过点拉动工位不能为空");
			}
			if (data.Any(t => t.PassPullAdvanceTime == null ))
			{
				return new Outcome(500, "过点拉动提前时间不能为空");
			}
			if (data.Any(t => t.SequenceNum == null))
			{
				return new Outcome(500, "顺序号不能为空");
			}
			var typeData = new T_MES_StationData();
			var sql = $"select * from MES_Station where StationCode in ('{string.Join("','", data.Select(t => t.StationCode))}') And ProductLineCode in ('{string.Join("','",data.Select(t=>t.ProductlineCode))}')";
			var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.MES_Station.TableName);
			typeData.Merge(typeData.MES_Station.TableName, ds);
			foreach (var item in data)
			{
				var row = typeData.MES_Station.Where(t=>t.StationCode==item.StationCode && t.ProductLineCode==item.ProductlineCode.ConvertString()).FirstOrDefault();
				if (row != null)
				{
					row.Version = row["Version"].ConvertInt32() + 1;
					row.ModifyBy = "WC2API";
					row.ModifyDate = GlobalContext.ServerTime;
					row.StationName = item.StationName;
					row.SegmentCode = item.StationRange;
					row.Interval = item.IntervalTime.ConvertInt32();
					row.PullStationCode = item.PassPullStationCode;
					row.PullTime = item.PassPullAdvanceTime.ConvertInt32();
					row.StationOrder = item.SequenceNum.ConvertInt32();
				}
				else
				{
					row = typeData.MES_Station.NewMES_StationRow();
					row.Version = 1;
					row.CreateBy = "WC2API";
					row.CreateDate = GlobalContext.ServerTime;
					row.StationCode = item.StationCode;

					row.StationName = item.StationName;
					row.SegmentCode = item.StationRange;
					row.ProductLineCode = item.ProductlineCode.ConvertString();
					row.Interval = item.IntervalTime.ConvertInt32();
					row.PullStationCode = item.PassPullStationCode;
					row.PullTime = item.PassPullAdvanceTime.ConvertInt32();
					row.StationOrder = item.SequenceNum.ConvertInt32();
					typeData.MES_Station.AddMES_StationRow(row);
				}
			}
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
		}
    }
}