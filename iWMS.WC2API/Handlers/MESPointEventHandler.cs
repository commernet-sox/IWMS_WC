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
	/// 投料点
	/// </summary>
	public class MESPointEventHandler : ISyncListEventHandler<MESPointRequest, Outcome>
    {
        public Outcome Handle(IEnumerable<MESPointRequest> data)
        {
			if (data == null || !data.Any())
			{
				return new Outcome(400, "数据列表为空");
			}
			if (data.Any(t => t.FeedingPointName == null || t.FeedingPointName == ""))
			{
				return new Outcome(400, "投料点名称不能为空");
			}
			if (data.Any(t => t.FeedingPointCode == null || t.FeedingPointCode == ""))
			{
				return new Outcome(400, "投料点编码不能为空");
			}
			if (data.Any(t => t.ProductlineCode == null || t.ProductlineCode==""))
			{
				return new Outcome(400, "所属产线编码不能为空");
			}
			if (data.Any(t => t.CarGroupCode == null || t.CarGroupCode==""))
			{
				return new Outcome(400, "所属装车组编码不能为空");
			}
			//if (data.Any(t => t.FeedingZone == null || t.FeedingZone == ""))
			//{
			//	return new Outcome(400, "投料区域不能为空");
			//}
			if (data.Any(t => t.Property == null || t.Property == ""))
			{
				return new Outcome(400, "投料点属性不能为空");
			}
			if (data.Any(t => t.IsParent == null))
			{
				return new Outcome(400, "是否父投料点不能为空");
			}
			var typeData = new T_MES_PointData();
			var sql = $"select * from MES_Point where PointCode in ('{string.Join("','", data.Select(t => t.FeedingPointCode))}') And ProductLineCode in ('{string.Join("','",data.Select(t=>t.ProductlineCode))}')";
			var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.MES_Point.TableName);
			typeData.Merge(typeData.MES_Point.TableName, ds);
			foreach (var item in data)
			{
				var row = typeData.MES_Point.Where(t=>t.PointCode==item.FeedingPointCode && t.ProductLineCode== item.ProductlineCode.ConvertString()).FirstOrDefault();
				if (row != null)
				{
					row.Version = row["Version"].ConvertInt32() + 1;
					row.ModifyBy = "WC2API";
					row.ModifyDate = GlobalContext.ServerTime;
					row.PointName = item.FeedingPointName;
					row.CartGroupCode = item.CarGroupCode.ConvertString();
					row.AreaId = item.FeedingZone;
					row.Property = item.Property;
					row.IsParent = item.IsParent.ConvertInt32();
					row.ParentCode = item.ParentCode.ConvertString();
				}
				else
				{
					row = typeData.MES_Point.NewMES_PointRow();
					row.Version = 1;
					row.CreateBy = "WC2API";
					row.CreateDate = GlobalContext.ServerTime;
					row.PointCode = item.FeedingPointCode;
					row.ProductLineCode = item.ProductlineCode.ConvertString();
					row.PointName = item.FeedingPointName;
					row.CartGroupCode = item.CarGroupCode.ConvertString();
					row.AreaId = item.FeedingZone;
					row.Property = item.Property;
					row.IsParent = item.IsParent.ConvertInt32();
					row.ParentCode = item.ParentCode.ConvertString();
					typeData.MES_Point.AddMES_PointRow(row);
				}
			}
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
		}
    }
}