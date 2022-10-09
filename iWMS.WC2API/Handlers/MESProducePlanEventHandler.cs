using Common.ISource;
using FWCore;
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
    /// MES生产计划
    /// </summary>
    public class MESProducePlanEventHandler : ISyncListEventHandler<MESProducePlanRequest, Outcome>
    {
		public Outcome Handle(IEnumerable<MESProducePlanRequest> data)
		{
			if (data == null || !data.Any())
			{
				return new Outcome(400, "数据列表为空");
			}
			if (data.Any(t => t.BomHeaderId == null || t.BomHeaderId == ""))
			{
				return new Outcome(400, "BOM头id不能为空");
			}
			if (data.Any(t => t.WipEntityName == null || t.WipEntityName == ""))
			{
				return new Outcome(400, "工单编码不能为空");
			}
			if (data.Any(t => t.MesPlanOrder == null || t.MesPlanOrder == ""))
			{
				return new Outcome(400, "计划顺序不能为空");
			}
			if (data.Any(t => t.ItemCode == null || t.ItemCode == ""))
			{
				return new Outcome(400, "物料编码不能为空");
			}
			if (data.Any(t => t.ProductType == null || t.ProductType == ""))
			{
				return new Outcome(400, "机型不能为空");
			}
			if (data.Any(t => t.ProductCode == null || t.ProductCode == ""))
			{
				return new Outcome(400, "序列号不能为空");
			}
			if (data.Any(t => t.ErpStartDate == null ))
			{
				return new Outcome(400, "ERP计划开始时间不能为空");
			}
			if (data.Any(t => t.IsLocked == null || t.IsLocked == ""))
			{
				return new Outcome(400, "3为锁定，1为未锁定不能为空");
			}
			var typeData = new T_MES_ProducePlanData();
			var sql = $"select * from MES_ProducePlan where OrderNo in ('{string.Join("','", data.Select(t => t.WipEntityName))}')";
			var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.MES_ProducePlan.TableName);
			typeData.Merge(typeData.MES_ProducePlan.TableName, ds);
			foreach (var item in data)
			{
				var row = typeData.MES_ProducePlan.Where(t=>t.OrderNo==item.WipEntityName).FirstOrDefault();
				if (row != null)
				{
					row.Version = row["Version"].ConvertInt32() + 1;
					row.ModifyBy = "WC2API";
					row.ModifyDate = GlobalContext.ServerTime;
					row.BOMID = item.BomHeaderId;
					row.OrderId = item.WipEntityId;
					row.ProductOrder = item.MesPlanOrder.ConvertLong();
					row.SkuId = item.ItemCode;
					row.Model = item.ProductType;
					row.ProductCode = item.ProductCode;
					row.StartDate = item.ErpStartDate??DateTime.Now;
					row.IsLocked = item.IsLocked;

				}
				else
				{
					row = typeData.MES_ProducePlan.NewMES_ProducePlanRow();
					row.Version = 1;
					row.CreateBy = "WC2API";
					row.CreateDate = GlobalContext.ServerTime;
					row.OrderNo = item.WipEntityName;
					row.Status = "10";

					row.BOMID = item.BomHeaderId;
					row.OrderId = item.WipEntityId;
					row.ProductOrder = item.MesPlanOrder.ConvertLong();
					row.SkuId = item.ItemCode;
					row.Model = item.ProductType;
					row.ProductCode = item.ProductCode;
					row.StartDate = item.ErpStartDate ?? DateTime.Now;
					row.IsLocked = item.IsLocked;
					typeData.MES_ProducePlan.AddMES_ProducePlanRow(row);
				}
			}
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
		}
	}
}