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
	/// 装车组
	/// </summary>
	public class MESCarGroupEventHandler : ISyncListEventHandler<MESCarGroupRequest, Outcome>
    {
        public Outcome Handle(IEnumerable<MESCarGroupRequest> data)
        {
			if (data == null || !data.Any())
			{
				return new Outcome(500, "数据列表为空");
			}
			if (data.Any(t => t.CarGroupName == null || t.CarGroupName == ""))
			{
				return new Outcome(500, "装车组名称不能为空");
			}
			if (data.Any(t => t.CarGroupCode == null || t.CarGroupCode == ""))
			{
				return new Outcome(500, "装车组编码不能为空");
			}
			if (data.Any(t => t.LoadArea == null || t.LoadArea == ""))
			{
				return new Outcome(500, "发货区不能为空");
			}
			if (data.Any(t => t.UnloadArea == null || t.UnloadArea == ""))
			{
				return new Outcome(500, "卸货区不能为空");
			}
			if (data.Any(t => t.BatchNum == null))
			{
				return new Outcome(500, "汇总波次不能为空");
			}
			if (data.Any(t => t.WaveNum == null))
			{
				return new Outcome(500, "投料台数不能为空");
			}
			if (data.Any(t => t.ProductlineCode == null))
			{
				return new Outcome(500, "所属产线编码不能为空");
			}

			var typeData = new T_MES_CarGroupData();
			var sql = $"select * from MES_CarGroup where CarGroupCode in ('{string.Join("','", data.Select(t => t.CarGroupCode))}') And ProductLineCode in ('{string.Join("','",data.Select(t=>t.ProductlineCode))}')";
			var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.MES_CarGroup.TableName);
			typeData.Merge(typeData.MES_CarGroup.TableName, ds);
			foreach (var item in data)
			{
				var row = typeData.MES_CarGroup.Where(t=>t.CarGroupCode==item.CarGroupCode && t.ProductLineCode==item.ProductlineCode.ConvertString()).FirstOrDefault();
				if (row != null)
				{
					row.Version = row["Version"].ConvertInt32() + 1;
					row.ModifyBy = "WC2API";
					row.ModifyDate = GlobalContext.ServerTime;
					row.CarGroupName = item.CarGroupName;
					row.LoadArea = item.LoadArea;
					row.UnloadArea = item.UnloadArea;
					row.BatchNum = item.BatchNum.ConvertString();
					row.WaveNum = item.WaveNum.ConvertString();
					row.Memo = item.Remark;
				}
				else
				{
					row = typeData.MES_CarGroup.NewMES_CarGroupRow();
					row.Version = 1;
					row.CreateBy = "WC2API";
					row.CreateDate = GlobalContext.ServerTime;
					row.CarGroupCode = item.CarGroupCode;
					row.ProductLineCode = item.ProductlineCode.ConvertString();
					row.CarGroupName = item.CarGroupName;
					row.LoadArea = item.LoadArea;
					row.UnloadArea = item.UnloadArea;
					row.BatchNum = item.BatchNum.ConvertString();
					row.WaveNum = item.WaveNum.ConvertString();
					row.Memo = item.Remark;
					typeData.MES_CarGroup.AddMES_CarGroupRow(row);
				}
			}
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
		}
    }
}