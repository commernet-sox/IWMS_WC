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
	/// 器具类型
	/// </summary>
	public class ContainerEventHandler : ISyncListEventHandler<ContainerRequest, Outcome>
	{
		public Outcome Handle(IEnumerable<ContainerRequest> data)
		{
			if (data == null || !data.Any())
			{
				return new Outcome(400, "数据列表为空");
			}
			if (data.Any(t => t.Code == null || t.Code == ""))
			{
				return new Outcome(400, "器具类型编码不能为空");
			}
			if (data.Any(t => t.ApplianceTypeName == null || t.ApplianceTypeName == ""))
			{
				return new Outcome(400, "器具类型名称不能为空");
			}
			var typeData = new T_COM_ContainerTypeData();
			var sql = $"select * from COM_ContainerType where CTypeCode in ('{string.Join("','", data.Select(t => t.Code))}')";
			var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.COM_ContainerType.TableName);
			typeData.Merge(typeData.COM_ContainerType.TableName, ds);
			foreach (var item in data)
			{
				var row = typeData.COM_ContainerType.FindByCTypeCode(item.Code);
				if (row != null)
				{
					row.Version = row["Version"].ConvertInt32() + 1;
					row.ModifyBy = "WC2API";
					row.ModifyDate = GlobalContext.ServerTime;
					row.CTypeName = item.ApplianceTypeName;
					row.Capacity = item.FeedPackaging.ConvertInt32();
					row.Status = item.Removed == 1 ? 0 : 1;
				}
				else
				{
					row = typeData.COM_ContainerType.NewCOM_ContainerTypeRow();
					row.Version = 1;
					row.CreateBy = "WC2API";
					row.CreateDate = GlobalContext.ServerTime;
					row.CTypeCode = item.Code;
					row.CTypeName = item.ApplianceTypeName;
					row.Capacity = item.FeedPackaging.ConvertInt32();
					row.Status = item.Removed == 1 ? 0 : 1;
					typeData.COM_ContainerType.AddCOM_ContainerTypeRow(row);
				}
			}
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
		}
	}
}