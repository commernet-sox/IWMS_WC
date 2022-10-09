using Common.ISource;
using FWCore;
using iWMS.BlueSkyAPI.Models;
using iWMS.Core;
using iWMS.TypedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.WebApplication;

namespace iWMS.BlueSkyAPI.Handlers
{
    /// <summary>
    /// 盘点单创建任务
    /// </summary>
    public class WRMInventoryEventHandler : ISyncEventHandler<WRMInventoryRequest, Outcome>
    {
        public Outcome Handle(WRMInventoryRequest data)
        {
            if (data == null)
            {
                return new Outcome(400, "数据列表为空");
            }
            if (string.IsNullOrWhiteSpace(data.OrderCode))
            {
                return new Outcome(400, "盘点单编号不能为空");
            }
            if (string.IsNullOrWhiteSpace(data.StorerId))
            {
                return new Outcome(400, "货主代码不能为空");
            }
            if (string.IsNullOrWhiteSpace(data.InventoryMode))
            {
                return new Outcome(400, "盘点方式不能为空");
            }
            if (string.IsNullOrWhiteSpace(data.InventoryType))
            {
                return new Outcome(400, "盘点分类不能为空");
            }
            if ((data.Items == null && !data.Items.Any())|| data.Items.Count() == 0)
            {
                return new Outcome(400, "盘点单明细不能为空");
            }

            string[] skuStatuses = new string[2] { "AVL", "DAMAGE" };

            if (data.Items.Any(t => t.SkuId == null || t.SkuId == ""))
            {
                return new Outcome(400, "物料编码不能为空");
            }
            if (data.Items.Any(t => t.SkuStatus == null || t.SkuStatus == ""))
            {
                return new Outcome(400, "物料状态不能为空");
            }
            if (data.Items.Any(t => !skuStatuses.Contains(t.SkuStatus)))
            {
                return new Outcome(400, "物料状态数据错误");
            }
            string[] inventoryModes = new string[3] { "AGV", "IMPORT", "PDA" };
            if (!inventoryModes.Contains(data.InventoryMode))
            {
                return new Outcome(400, "盘点方式数据错误");
            }
            string[] inventoryTypes = new string[4] { "MT", "OVERALL", "RANDOM", "SPECIAL" };
            if (!inventoryTypes.Contains(data.InventoryType))
            {
                return new Outcome(400, "盘点类型数据错误");
            }

            var noDistinct = data.Items.GroupBy(x => x.OrderItemId).All(x => x.Count() == 1);
            if (!noDistinct)
            {
                return new Outcome(400, "订单行号重复");
            }

            var typeData = new T_WRM_InventoryData();
            var sql = $"select * from WRM_Inventory where OrigBillId = '{data.OrderCode}'";
            var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.WRM_Inventory.TableName);
            typeData.Merge(typeData.WRM_Inventory.TableName, ds);

            //不允许更新，以防止其他程序操作后造成数据异常。
            List<string> error = new List<string>();
            if (typeData.WRM_Inventory.Count > 0)
            {
                error = typeData.WRM_Inventory.Select(t => t.OrigBillId).ToList();
                return new Outcome(400, $"{string.Join(",", error)} 盘点单已存在，请重新输入！");
            }

            var row = typeData.WRM_Inventory.NewWRM_InventoryRow();
            //获取BillId  
            var asn_BillId = GlobalContext.Resolve<ISource_BillHelper>().GetBillId("PD", Commons.StoreId, Commons.WarehouseId);
            HttpContext.Current.SetLogId(asn_BillId);

            row.BillId = asn_BillId;
            row.StorerId = data.StorerId;
            row.WarehouseId = "07";
            row.Status = "10";
            row.InventoryType = data.InventoryType;
            row.OrderDate = data.OrderDate;
            row.OrigBillId = data.OrderCode;
            row.InventoryMode = data.InventoryMode;
            row.BeginDate = data.BeginDate;
            row.EndDate = data.EndDate;
            row.Version = 0;
            row.CreateBy = "System";
            row.CreateDate = GlobalContext.ServerTime;
            row.Memo = data.Memo;
            typeData.WRM_Inventory.AddWRM_InventoryRow(row);

            foreach (var item in data.Items)
            {
                var rowDtl = typeData.WRM_InventoryRange.NewWRM_InventoryRangeRow();
                rowDtl.BillId = row.BillId;
                rowDtl.CpType = "SKU";
                rowDtl.SkuId = item.SkuId;
                rowDtl.Version = 0;
                rowDtl.CreateBy = "System";
                rowDtl.CreateDate = GlobalContext.ServerTime;
                typeData.WRM_InventoryRange.AddWRM_InventoryRangeRow(rowDtl);
            }

            GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
            typeData.AcceptChanges();
            return new Outcome();
        }
    }

}