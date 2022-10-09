using Common.ISource;
using FWCore;
using iWMS.Core;
using iWMS.TypedData;
using iWMS.WCDigitAPI.Core;
using iWMS.WCDigitAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iWMS.WCDigitAPI.Handlers
{
    public class ERPSRMOrderEventHandler : ISyncEventHandler<ERPSRMOrderRequest, Outcome>
    {
        public Outcome Handle(ERPSRMOrderRequest data)
        {
			if (data == null ||  !data.PAsnLine.Any() || !data.PAsnHead.Any())
			{
				return new Outcome(500, "数据列表为空");
			}
			if (data.PAsnHead.Any(t => t.OrgId == null || t.OrgId == ""))
			{
				return new Outcome(500, "业务实体ID不能为空");
			}
			if (data.PAsnHead.Any(t => t.ShipmentHeaderId == null || t.ShipmentHeaderId == ""))
			{
				return new Outcome(500, "发运头表ID不能为空");
			}
			if (data.PAsnHead.Any(t => t.ShipmentNum == null || t.ShipmentNum == ""))
			{
				return new Outcome(500, "ASN号不能为空");
			}
			if (data.PAsnHead.Any(t => t.VendorsCode == null || t.VendorsCode == ""))
			{
				return new Outcome(500, "供方代码不能为空");
			}
			if (data.PAsnHead.Any(t => t.VendorsName == null || t.VendorsName == ""))
			{
				return new Outcome(500, "供方名称不能为空");
			}
			if (data.PAsnHead.Any(t => t.LastUpdateDate == null || t.LastUpdateDate == ""))
			{
				return new Outcome(500, "最后更新时间不能为空");
			}
			if (data.PAsnHead.Any(t => t.ExpectedReceiptDate == null || t.ExpectedReceiptDate == ""))
			{
				return new Outcome(500, "到货时间不能为空");
			}


			if (data.PAsnLine.Any(t => t.OrganizationId == null || t.OrganizationId == ""))
			{
				return new Outcome(500, "库存组织不能为空");
			}
			if (data.PAsnLine.Any(t => t.ShipmentHeaderId == null || t.ShipmentHeaderId == ""))
			{
				return new Outcome(500, "发运头表ID不能为空");
			}
			if (data.PAsnLine.Any(t => t.ShipmentNum == null || t.ShipmentNum == ""))
			{
				return new Outcome(500, "ASN号不能为空");
			}
			if (data.PAsnLine.Any(t => t.PoheadId == null || t.PoheadId == ""))
			{
				return new Outcome(500, "采购订单头表ID不能为空");
			}
			if (data.PAsnLine.Any(t => t.OrderCode == null || t.OrderCode == ""))
			{
				return new Outcome(500, "采购订单号不能为空");
			}
			if (data.PAsnLine.Any(t => t.VendorsId == null || t.VendorsId == ""))
			{
				return new Outcome(500, "供方ID不能为空");
			}
			if (data.PAsnLine.Any(t => t.PolineId == null || t.PolineId == ""))
			{
				return new Outcome(500, "采购订单行表ID不能为空");
			}
			if (data.PAsnLine.Any(t => t.OrderRowNum == null || t.OrderRowNum == ""))
			{
				return new Outcome(500, "采购订单行号不能为空");
			}
			if (data.PAsnLine.Any(t => t.ItemId == null || t.ItemId == ""))
			{
				return new Outcome(500, "物料ID不能为空");
			}
			if (data.PAsnLine.Any(t => t.ItemCode == null || t.ItemCode == ""))
			{
				return new Outcome(500, "物料件号不能为空");
			}
			if (data.PAsnLine.Any(t => t.ItemDescription == null || t.ItemDescription == ""))
			{
				return new Outcome(500, "物料描述不能为空");
			}
			if (data.PAsnLine.Any(t => t.ReceivingRoutingId == null || t.ReceivingRoutingId == ""))
			{
				return new Outcome(500, "ASN检验标识不能为空");
			}
			if (data.PAsnLine.Any(t => t.QuantityShipped == null || t.QuantityShipped == ""))
			{
				return new Outcome(500, "发运数量不能为空");
			}
			if (data.PAsnLine.Any(t => t.ExpectedReceiptDate == null || t.ExpectedReceiptDate == ""))
			{
				return new Outcome(500, "到货时间不能为空");
			}
			if (data.PAsnLine.Any(t => t.LastUpdateDate == null || t.LastUpdateDate == ""))
			{
				return new Outcome(500, "最后更新时间不能为空");
			}

			var typeData = new T_SRM_OrderData();
			var sql = $"select * from SRM_Order where SyncBillId in ('{string.Join("','", data.PAsnHead.Select(t => t.ShipmentNum))}') ";
			var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SRM_Order.TableName);
			typeData.Merge(typeData.SRM_Order.TableName, ds);

			//不允许更新，以防止其他程序操作后造成数据异常。
			List<string> error = new List<string>();
			if (typeData.SRM_Order.Count > 0)
			{
				error = typeData.SRM_Order.Select(t => t.SyncBillId).ToList();
				return new Outcome(500, $"{string.Join(",", error)} ASN入库单已存在，请重新输入！");
			}

			foreach (var head in data.PAsnHead)
			{
				var billId = GlobalContext.Resolve<ISource_BillHelper>().GetBillId("",Commons.StoreId,Commons.WarehouseId);
				//一个asn只对应一条sku
				var line = data.PAsnLine.Where(t => t.ShipmentNum == head.ShipmentNum).FirstOrDefault();
				var row = typeData.SRM_Order.NewSRM_OrderRow();
				var rowDtl= typeData.SRM_OrderDetail.NewSRM_OrderDetailRow();
				row.Version = 1;
				row.CreateBy = "WC2API";
				row.CreateDate = GlobalContext.ServerTime;
				row.BillId = billId;
				row.UDF01 = head.OrgId;
				row.UDF02 = head.ShipmentHeaderId;
				row.SyncBillId = head.ShipmentNum;
				row.UDF03 = head.VendorsCode;
				row.UDF04 = head.VendorsName;
				row.UDF05 = head.PoheadId;
				row.UDF06 = head.PoreleaseId;
				row.UDF07 = head.OrderCode;
				row.UDF08 = head.ReleaseNum;
				row.UDF09 = head.LastUpdateDate;
				row.UDF10 = head.ExpectedReceiptDate;
				row.UDF14 = line.ReceivingRoutingId;
				typeData.SRM_Order.AddSRM_OrderRow(row);
				rowDtl.Version = 1;
				rowDtl.CreateBy = "WC2API";
				rowDtl.CreateDate = GlobalContext.ServerTime;
				rowDtl.UDF01 = line.OrganizationId;
				rowDtl.UDF02 = line.ShipmentHeaderId;
				rowDtl.BillId = billId;
				rowDtl.UDF03 = line.PoheadId;
				rowDtl.UDF04 = line.PoreleaseId;
				rowDtl.UDF05 = line.OrderCode;
				rowDtl.UDF06 = line.ReleaseNum;
				rowDtl.UDF07 = line.PolineId;
				rowDtl.UDF08 = line.OrderRowNum;
				rowDtl.UDF09 = line.ItemId;
				rowDtl.SkuId = line.ItemCode;
				rowDtl.UDF10 = line.ItemDescription;
				rowDtl.UDF11 = line.Measure;
				rowDtl.QtyPlan = line.QuantityShipped.ConvertDecimal();
				typeData.SRM_OrderDetail.AddSRM_OrderDetailRow(rowDtl);
			}
			GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
			typeData.AcceptChanges();
			return new Outcome();
		}
    }
}