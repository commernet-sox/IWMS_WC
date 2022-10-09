using Common.DBCore;
using FWCore;
using iWMS.TypedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.Core;
using Common.ISource;
using iWMS.WCDigitAPI.Models;

namespace iWMS.WCDigitAPI.Handlers
{
	/// <summary>
	/// 产线管理
	/// </summary>
	public class MESProductLineEventHandler : ISyncListEventHandler<MESProductLineRequest, Outcome>
    {
        public Outcome Handle(IEnumerable<MESProductLineRequest> data)
        {
			if (data == null || !data.Any())
			{
				return new Outcome(500, "数据列表为空");
			}
			if (data.Any(t => t.ProductlineCode == null || t.ProductlineCode==""))
			{
				return new Outcome(500, "产线编码不能为空");
			}
			if (data.Any(t => t.ProductlineName == null || t.ProductlineName==""))
			{
				return new Outcome(500, "产线名称不能为空");
			}
			if (data.Any(t => t.BatchNum == null))
			{
				return new Outcome(500, "汇总波次不能为空");
			}
			if (data.Any(t => t.FeedingNumber == null))
			{
				return new Outcome(500, "投料台数不能为空");
			}

			var typeData = new T_MES_ProductLineTypeData();
			var sql = $"select * from MES_ProductLine where ProductLineCode in ('{string.Join("','", data.Select(t => t.ProductlineCode))}')";
			var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.MES_ProductLine.TableName);
			typeData.Merge(typeData.MES_ProductLine.TableName, ds);
			foreach (var item in data)
			{
				var row = typeData.MES_ProductLine.FindByProductLineCode(item.ProductlineCode);
				if (row != null)
				{
					row.Version = row["Version"].ConvertInt32() + 1;
					row.ModifyBy = "WC2API";
					row.ModifyDate = GlobalContext.ServerTime;
					row.ProduceName = item.ProductlineName;
					row.BatchNum = item.BatchNum.ConvertInt32();
					row.FeedingNumber = item.FeedingNumber.ConvertInt32();
				}
				else
				{
					row = typeData.MES_ProductLine.NewMES_ProductLineRow();
					row.Version = 1;
					row.CreateBy = "WC2API";
					row.CreateDate = GlobalContext.ServerTime;
					row.ProductLineCode = item.ProductlineCode;
					row.ProduceName = item.ProductlineName;
					row.BatchNum = item.BatchNum.ConvertInt32();
					row.FeedingNumber = item.FeedingNumber.ConvertInt32();
					typeData.MES_ProductLine.AddMES_ProductLineRow(row);
				}
			}
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
        }
    }
}