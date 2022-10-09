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
    /// MES BOM
    /// </summary>
    public class MESBOMEventHandler : ISyncListEventHandler<MESBOMRequest, Outcome>
    {
		public Outcome Handle(IEnumerable<MESBOMRequest> data)
		{
			if (data == null || !data.Any())
			{
				return new Outcome(400, "数据列表为空");
			}
			if (data.Any(t => t.BomHeaderId == null || t.BomHeaderId == ""))
			{
				return new Outcome(400, "BOM头id不能为空");
			}
			if (data.Any(t => t.ItemId == null || t.ItemId == ""))
			{
				return new Outcome(400, "物料id不能为空");
			}
			
			var typeData = new T_MES_BOMData();
			var sql = $"select * from MES_BOM where BOMID in ('{string.Join("','", data.Select(t => t.BomHeaderId))}')";
			var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.MES_BOM.TableName);
			typeData.Merge(typeData.MES_BOM.TableName, ds);
			foreach (var item in data)
			{
				var row = typeData.MES_BOM.Where(t => t.BOMID == item.BomHeaderId).FirstOrDefault();
				if (row != null)
				{
					row.Version = row["Version"].ConvertInt32() + 1;
					row.ModifyBy = "WC2API";
					row.ModifyDate = GlobalContext.ServerTime;
					

					row.Ex1 = item.ItemId;
					row.SkuId = item.ItemCode;
					row.Qty = item.Quantity.ConvertDecimal();
					row.KeyFlag = item.KeyFlag;
					row.Ex2 = item.OperationId;
					row.StationCode = item.OperationCode;
					row.Ex3 = item.AssignedSupplierId;
					row.SupplierCode = item.AssignedSupplierCode;
					row.RowId = item.RowId;
				}
				else
				{
					row = typeData.MES_BOM.NewMES_BOMRow();
					row.Version = 1;
					row.CreateBy = "WC2API";
					row.CreateDate = GlobalContext.ServerTime;
					row.BOMID = item.BomHeaderId;
					
					row.Ex1 = item.ItemId;
					row.SkuId = item.ItemCode;
					row.Qty = item.Quantity.ConvertDecimal();
					row.KeyFlag = item.KeyFlag;
					row.Ex2 = item.OperationId;
					row.StationCode = item.OperationCode;
					row.Ex3 = item.AssignedSupplierId;
					row.SupplierCode = item.AssignedSupplierCode;
					row.RowId = item.RowId;
					typeData.MES_BOM.AddMES_BOMRow(row);
				}
			}
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
		}
	}
}