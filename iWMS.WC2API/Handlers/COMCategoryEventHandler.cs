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
	/// 物料分类
	/// </summary>
	public class COMCategoryEventHandler : ISyncListEventHandler<COMCategoryRequest, Outcome>
	{
		public Outcome Handle(IEnumerable<COMCategoryRequest> data)
		{
			if (data == null || !data.Any())
			{
				return new Outcome(400, "数据列表为空");
			}
			if (data.Any(t => t.CategoryCode == null || t.CategoryCode == ""))
			{
				return new Outcome(400, "物料分类编码不能为空");
			}
			if (data.Any(t => t.CategoryName == null || t.CategoryName == ""))
			{
				return new Outcome(400, "物料分类名称不能为空");
			}
			if (data.Any(t => t.SpecificationName == null || t.SpecificationName == ""))
			{
				return new Outcome(400, "物料规格不能为空");
			}
			if (data.Any(t => t.ProductLineCode == null || t.ProductLineCode == ""))
			{
				return new Outcome(400, "产线编码不能为空");
			}
			if (data.Any(t => t.ProductLineName == null || t.ProductLineName == ""))
			{
				return new Outcome(400, "产线名称不能为空");
			}
			var typeData = new T_COM_CategoryData();
			var sql = $"select * from COM_Category where CategoryId in ('{string.Join("','", data.Select(t => t.CategoryCode))}')";
			var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.COM_Category.TableName);
			typeData.Merge(typeData.COM_Category.TableName, ds);
			foreach (var item in data)
			{
				var row = typeData.COM_Category.FindByCategoryId(item.CategoryCode);
				if (row != null)
				{
					row.Version = row["Version"].ConvertInt32() + 1;
					row.ModifyBy = "WC2API";
					row.ModifyDate = GlobalContext.ServerTime;
					row.CategoryName = item.CategoryName;
					row.Ex1 = item.SpecificationName;
					row.Ex2 = item.ProductLineCode;
					row.Ex3 = item.ProductLineName;
				}
				else
				{
					row = typeData.COM_Category.NewCOM_CategoryRow();
					row.Version = 1;
					row.CreateBy = "WC2API";
					row.CreateDate = GlobalContext.ServerTime;
					row.CategoryName = item.CategoryName;
					row.CategoryId = item.CategoryCode;
					row.ParentId = "";
					row.Layer = 10;
					row.IsRoot = 10;
					row.IsFoot = 10;
					row.Ex1 = item.SpecificationName;
					row.Ex2 = item.ProductLineCode;
					row.Ex3 = item.ProductLineName;
					typeData.COM_Category.AddCOM_CategoryRow(row);
				}
			}
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
		}
	}
}