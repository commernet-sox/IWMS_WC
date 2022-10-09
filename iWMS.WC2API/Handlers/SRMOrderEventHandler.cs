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
using iWMS.WC2API.Core;
using iWMS.WebApplication;

namespace iWMS.WC2API.Handlers
{
	/// <summary>
	/// 入库任务
	/// </summary>
	public class SRMOrderEventHandler : ISyncListEventHandler<SRMOrderRequest, Outcome>
	{
		public Outcome Handle(IEnumerable<SRMOrderRequest> data)
		{
			if (data == null || !data.Any())
			{
				return new Outcome(400, "数据列表为空");
			}
			if (data.Any(t => t.InputSerial == null || t.InputSerial == ""))
			{
				return new Outcome(400, "入库单号不能为空");
			}
			if (data.Any(t => t.Appliance == null || t.Appliance == ""))
			{
				return new Outcome(400, "器具编码不能为空");
			}
			if (data.Any(t => t.FeedingPointCode == null || t.FeedingPointCode == ""))
			{
				return new Outcome(400, "投料点编码不能为空");
			}
			if (data.Any(t => t.FeedingPointName == null || t.FeedingPointName == ""))
			{
				return new Outcome(400, "投料点名称不能为空");
			}
			if (data.Any(t => t.Status == null || t.Status == ""))
			{
				return new Outcome(400, "任务状态不能为空");
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
			if (data.Any(t => t.WaveNum == null || t.WaveNum == ""))
			{
				return new Outcome(400, "波次不能为空");
			}
			if (data.Any(t => t.BatchNum == null || t.BatchNum == ""))
			{
				return new Outcome(400, "批次不能为空");
			}
			if (data.Any(t => t.MaterialCode == null || t.MaterialCode == ""))
			{
				return new Outcome(400, "物料编码不能为空");
			}
			if (data.Any(t => t.MaterialName == null || t.MaterialName == ""))
			{
				return new Outcome(400, "物料名称不能为空");
			}
			if (data.Any(t => t.Quantity == null))
			{
				return new Outcome(400, "数量不能为空");
			}
			if (data.Any(t => t.StationCode == null || t.StationCode == ""))
			{
				return new Outcome(400, "工位编码不能为空");
			}
			if (data.Any(t => t.StationName == null || t.StationName == ""))
			{
				return new Outcome(400, "工位名称不能为空");
			}
			if (data.Any(t => t.PullNumber == null || t.PullNumber == ""))
			{
				return new Outcome(400, "拉动条码不能为空");
			}
			if (data.Any(t => t.PackSort == null ))
			{
				return new Outcome(400, "包装顺序不能为空");
			}
			//if (data.Any(t => t.LotNumber == null || t.LotNumber == ""))
			//{
			//	return new Outcome(400, "物料批次不能为空");
			//}
			//if (data.Any(t => t.VendorNumber == null || t.VendorNumber == ""))
			//{
			//	return new Outcome(400, "供应商编码不能为空");
			//}
			if (data.Any(t => t.UtensilTypeCode == null || t.UtensilTypeCode == ""))
			{
				return new Outcome(400, "器具类型编码不能为空");
			}
			if (data.Any(t => t.DetailId == null || t.DetailId == ""))
			{
				return new Outcome(400, "入库单明细id不能为空");
			}
			if (data.Any(t => t.CarGroupCode == null || t.CarGroupCode == ""))
			{
				return new Outcome(400, "装车组编码不能为空");
			}
			if (data.Any(t => t.CarGroupName == null || t.CarGroupName == ""))
			{
				return new Outcome(400, "装车组名称不能为空");
			}
			if (data.Any(t => t.LineWaveNum == null || t.LineWaveNum == ""))
			{
				return new Outcome(400, "产线波次不能为空");
			}

			var typeData = new T_SRM_OrderData();
			var sql = $"select * from SRM_Order where SyncBillId in ('{string.Join("','", data.Select(t => t.InputSerial))}')";
			var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SRM_Order.TableName);
			typeData.Merge(typeData.SRM_Order.TableName, ds);

			//不允许更新，以防止其他程序操作后造成数据异常。
			List<string> error = new List<string>();
			if (typeData.SRM_Order.Count > 0)
			{
				error = typeData.SRM_Order.Select(t => t.SyncBillId).ToList();
				return new Outcome(400, $"{string.Join(",", error)} 入库单已存在，请重新输入！");
			}

			var gList = data.GroupBy(t => t.InputSerial).ToList();
			foreach (var group in gList)
			{
				var syncBillId = group.Key;
				var Detail = group.ToList();

				var row = typeData.SRM_Order.NewSRM_OrderRow();
				//获取BillId
				var asn_BillId = GlobalContext.Resolve<ISource_BillHelper>().GetBillId("ASN", Commons.StoreId, Commons.WarehouseId);
				//GlobalContext.Resolve<ISource_SQLHelper>().ExecuteScalar($"DECLARE @Id VARCHAR(30) exec SP_GEN_ID 'ASN',{storeId},{warehouseId},@Id OUTPUT SELECT @Id").ConvertString();
				row.Version = 1;
				row.CreateBy = "WC2API";
				row.CreateDate = GlobalContext.ServerTime;
				row.BillId = asn_BillId;

				row.StorerId =Commons.StoreId;
				row.WarehouseId = Commons.WarehouseId;
				row.InOutType = 0;
				row.Status = "10";
				row.IsCharge = "0";
				row.BatchNo = Detail[0].BatchNum;
				row.SyncBillId = Detail[0].InputSerial;
				row.SyncBillDate = DateTime.Now;
				row.OrigBillType = 1;
				row.BusinessType = "NORMAL";
				row.OrigSystem = "LES";
				row.GoodsCount = Detail.GroupBy(t => t.MaterialCode).Count();
				row.SkuCount = Detail.Sum(t => t.Quantity).ConvertDecimal();
				row.CID = Detail[0].Appliance;
				row.UDF01 = Detail[0].Status;
				row.UDF02 = Detail[0].ProductLineCode;
				row.UDF03 = Detail[0].ProductLineName;
				row.UDF04 = Detail[0].PlanDate.ConvertString();
				row.UDF05 = Detail[0].CarGroupCode;
				row.UDF06 = Detail[0].CarGroupName;
				row.OrderDate = Detail[0].PlanDate??DateTime.Now;
				row.VendorId = Detail[0].VendorNumber;
				row.OrderType = "NORMAL";//正常入库
				row.UDF20 = "LES";
				typeData.SRM_Order.AddSRM_OrderRow(row);

				foreach (var item in Detail)
				{
					var rowDtl = typeData.SRM_OrderDetail.NewSRM_OrderDetailRow();
					rowDtl.Version = 1;
					rowDtl.CreateBy = "WC2API";
					rowDtl.CreateDate = GlobalContext.ServerTime;
					rowDtl.OrderLineNo = item.DetailId;
					rowDtl.StorerId = Commons.StoreId;
					rowDtl.BillId = row.BillId;
					rowDtl.SkuId = item.MaterialCode;
					rowDtl.SkuStatus = "AVL";
					rowDtl.BatchNo = item.LotNumber;
					rowDtl.PackageCode = "PCS";
					rowDtl.PackageQty = 1;
					rowDtl.UDF01 = item.FeedingPointCode;
					rowDtl.UDF02 = item.FeedingPointName;
					rowDtl.UDF03 = item.WaveNum;
					rowDtl.UDF04 = item.BatchNum;
					rowDtl.UDF05 = item.MaterialName;
					rowDtl.UDF06 = item.StationCode;
					rowDtl.UDF07 = item.StationName;
					rowDtl.UDF08 = item.PullNumber;
					rowDtl.UDF09 = item.LotNumber;
					rowDtl.UDF10 = item.UtensilTypeCode;
					rowDtl.UDF11 = item.CarGroupCode;
					rowDtl.UDF12 = item.CarGroupName;
					rowDtl.UDF13 = item.LineWaveNum;
					rowDtl.UDF14 = item.PackSort.ConvertString();
					rowDtl.UDF20 = Detail[0].Status;
					rowDtl.QtyPlan = item.Quantity.ConvertDecimal();
					typeData.SRM_OrderDetail.AddSRM_OrderDetailRow(rowDtl);
				}
			}
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
		}
	}
}