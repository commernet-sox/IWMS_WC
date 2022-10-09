using Common.ISource;
using FWCore;
using iWMS.Core;
using iWMS.TypedData;
using iWMS.WCDigitAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iWMS.WCDigitAPI.Handlers
{
	/// <summary>
	/// ASN质检结果下发
	/// </summary>
	public class WQSSRMOrderEventHandler : ISyncListEventHandler<WQSSRMOrderRequest, Outcome>
    {
        public Outcome Handle(IEnumerable<WQSSRMOrderRequest> data)
        {
			if (data == null || !data.Any())
			{
				return new Outcome(500, "数据列表为空");
			}
			if (data.Any(t => t.SourceRecId == null || t.SourceRecId == ""))
			{
				return new Outcome(500, "批次号不能为空");
			}
			if (data.Any(t => t.OrganizationCode == null || t.OrganizationCode == ""))
			{
				return new Outcome(500, "组织不能为空");
			}
			if (data.Any(t => t.ItemNumber == null || t.ItemNumber == ""))
			{
				return new Outcome(500, "物料编码不能为空");
			}
			if (data.Any(t => t.InspectResult == null || t.InspectResult == ""))
			{
				return new Outcome(500, "检验结果不能为空");
			}
			
			var typeData = new T_SRM_OrderData();
			var sql = $"select * from SRM_OrderDetail b on a.BillId=b.BillId where b.SkuId in ('{string.Join("','", data.Select(t => t.ItemNumber))}') And b.BatchNo in ('{string.Join("','", data.Select(t => t.SourceRecId))}')";
			var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SRM_OrderDetail.TableName);
			typeData.Merge(typeData.SRM_OrderDetail.TableName, ds);

			var srmOrderSql = $"select * from SRM_Order where BillId in ('{string.Join("','", typeData.SRM_OrderDetail.Select(t => t.BillId))}') ";
			var srmOrderDs = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SRM_Order.TableName);
			typeData.Merge(typeData.SRM_Order.TableName, srmOrderDs);

			foreach (var item in data)
			{
				var rowDtl = typeData.SRM_OrderDetail.Where(t => t.SkuId == item.ItemNumber && t.BatchNo == item.SourceRecId && t.UDF01==item.OrganizationCode).FirstOrDefault();
				if (rowDtl != null)
				{
					var row = typeData.SRM_Order.Where(t => t.BillId == rowDtl.BillId && t.UDF03==item.VendorCode).FirstOrDefault();
					if (row != null)
					{
						row.UDF12 = item.InspectResult;
						row.UDF13 = item.InspectDate;
						row.Version = row["Version"].ConvertInt32() + 1;
						row.ModifyBy = "WC2API";
						row.ModifyDate = GlobalContext.ServerTime;
					}
				}
				else
				{
					//未匹配到不处理
				}
			}
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
		}
    }
}