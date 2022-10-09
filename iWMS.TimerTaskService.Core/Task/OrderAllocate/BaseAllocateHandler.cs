using Common.DBCore;
using FWCore;
using iWMS.Entity;
using iWMS.TypedData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core.Task
{
    /// <summary>
    /// 订单分配处理逻辑基类
    /// </summary>
    public class BaseAllocateHandler
    {
        public virtual bool Allocate(DataRow row)
        {
            return false;
        }

        protected virtual bool CalcOrderTask(List<TRM_TaskXJDetail> tasks, DataRow row, bool allowPartialAllocate = true)
        {
            var taskTable = BusinessDbUtil.GetDataTable(string.Format(AllocateSQL.QueryOrderTaskDetailSql, row["BillId"]));
            if (taskTable == null || taskTable.Rows.Count < 1)
            {
                return true;
            }
            var orderTaskTable = BusinessDbUtil.GetDataTable(string.Format(AllocateSQL.OrderTaskDetailSql, row["BillID"]));
            if (orderTaskTable == null || orderTaskTable.Rows.Count < 1)
            {
                string errorMsg = "指定分配库存不足【数据异常】";
                BusinessDbUtil.ExecuteNonQuery(string.Format(AllocateSQL.UpdateOrderStatusSql,
                   row["BillId"], errorMsg, "106"));
                return false;
            }
            if (orderTaskTable != null && orderTaskTable.Rows.Count > 0)
            {
                if (!allowPartialAllocate)
                {
                    var groups = orderTaskTable.AsEnumerable()
                        .GroupBy(item => item["SkuId"].ToString() + item["BatchNo"].ToString());
                    foreach (var group in groups)
                    {
                        var taskRow = group.FirstOrDefault();
                        var qtyScan = group.Sum(item => item["QtyScan"].ConvertDecimal());
                        var qtyValid = group.Sum(item => item["QtyValid"].ConvertDecimal());
                        if (qtyScan > qtyValid)
                        {
                            string errorMsg = string.Format("指定分配库位{0},商品{1}库存不足！", taskRow["FromLoc"], taskRow["SkuId"]);
                            BusinessDbUtil.ExecuteNonQuery(string.Format(AllocateSQL.UpdateOrderStatusSql,
                               row["BillId"], errorMsg, "106"));
                            return false;
                        }
                    }
                }
            }
            foreach (DataRow taskRow in taskTable.Rows)
            {
                var orderTaskItem = orderTaskTable.AsEnumerable()
                    .FirstOrDefault(item =>
                    item["WarehouseId"].ToString() == taskRow["WarehouseId"].ToString()
                    && item["StorerId"].ToString() == taskRow["StorerId"].ToString()
                    && item["LocationId"].ToString() == taskRow["FromLoc"].ToString()
                    && item["SkuId"].ToString() == taskRow["SkuId"].ToString()
                    && item["CID"].ToString() == taskRow["CID"].ToString()
                    && item["BoxCode"].ToString() == taskRow["BoxCode"].ToString()
                     && item["BatchNo"].ToString() == taskRow["BatchNo"].ToString()
                      && item["SkuStatus"].ToString() == taskRow["SkuStatus"].ToString());
                var task = new TRM_TaskXJDetail
                {
                    OrderId = taskRow["OrderId"].ToString(),
                    OrderLineNo = taskRow["OrderLineNo"].ToString(),
                    StorerId = taskRow["StorerId"].ToString(),
                    WHAreaId = taskRow["WHAreaId"].ToString(),
                    AreaId = taskRow["AreaId"].ToString(),
                    FromLoc = taskRow["FromLoc"].ToString(),
                    ToLoc = taskRow["ToLoc"].ToString(),
                    FromCID = taskRow["CID"].ToString(),
                    BoxCode = taskRow["BoxCode"].ToString(),
                    BatchNo = taskRow["BatchNo"].ToString(),
                    SkuId = taskRow["SkuId"].ToString(),
                    SkuStatus = taskRow["SkuStatus"].ToString(),
                    Qty = taskRow["QtyScan"].ConvertDecimal(),
                    PackageCode = taskRow["PackageCode"].ToString(),
                    PackageQty = taskRow["PackageQty"].ConvertDecimal()
                };
                if (orderTaskItem != null)
                {
                    task.UDF01 = orderTaskItem["UDF01"].ToString();
                    task.UDF02 = orderTaskItem["UDF02"].ToString();
                    task.UDF03 = orderTaskItem["UDF03"].ToString();
                    task.UDF04 = orderTaskItem["UDF04"].ToString();
                    task.UDF05 = orderTaskItem["UDF05"].ToString();
                    task.DUDF01 = orderTaskItem["DUDF01"].ToString();
                    task.DUDF02 = orderTaskItem["DUDF02"].ToString();
                    task.DUDF03 = orderTaskItem["DUDF03"].ToString();
                    task.DUDF04 = orderTaskItem["DUDF04"].ToString();
                    task.DUDF05 = orderTaskItem["DUDF05"].ToString();
                    task.DUDF07 = orderTaskItem["DUDF07"].ToString();
                    task.DUDF08 = orderTaskItem["DUDF08"].ToString();
                    task.DUDF09 = orderTaskItem["DUDF09"].ToString();
                    task.DUDF10 = orderTaskItem["DUDF10"].ToString();
                    task.DUDF11 = orderTaskItem["DUDF11"].ToString();
                }
                tasks.Add(task);
            }
            return true;
        }

        protected virtual bool CalcOrderLineTask(List<TRM_TaskXJDetail> tasks, DataRow row, bool allowPartialAllocate = true)
        {
            var taskTable = BusinessDbUtil.GetDataTable(string.Format(AllocateSQL.QueryOrderTaskDetailSql, row["BillId"]));
            if (taskTable == null || taskTable.Rows.Count < 1)
            {
                return true;
            }
            var orderTaskTable = BusinessDbUtil.GetDataTable(string.Format(AllocateSQL.OrderTaskDetailOrderLineSql, row["BillID"]));
            if (orderTaskTable == null || orderTaskTable.Rows.Count < 1)
            {
                string errorMsg = "指定分配库存不足【数据异常】";
                BusinessDbUtil.ExecuteNonQuery(string.Format(AllocateSQL.UpdateOrderStatusSql,
                   row["BillId"], errorMsg, "106"));
                return false;
            }
            if (orderTaskTable != null && orderTaskTable.Rows.Count > 0)
            {
                if (orderTaskTable.AsEnumerable().Any(item => item["QtyValid"].ConvertInt32() <= 0))
                {
                    string errorMsg = string.Format("指定分配与库存数据不一致，分配失败！");
                    BusinessDbUtil.GetDataTable(string.Format(AllocateSQL.UpdateOrderLineStatusSql,
                       row["BillId"], "106"));
                    return false;
                }
                if (!allowPartialAllocate)
                {
                    var groups = orderTaskTable.AsEnumerable()
                        .GroupBy(item => item["SkuId"].ToString() + item["BatchNo"].ToString());
                    foreach (var group in groups)
                    {
                        var taskRow = group.FirstOrDefault();
                        var qtyScan = group.Sum(item => item["QtyScan"].ConvertDecimal());
                        var qtyValid = group.Sum(item => item["QtyValid"].ConvertDecimal());
                        if (qtyScan > qtyValid)
                        {
                            string errorMsg = string.Format("指定分配库位{0},商品{1}库存不足！", taskRow["FromLoc"], taskRow["SkuId"]);
                            BusinessDbUtil.ExecuteNonQuery(string.Format(AllocateSQL.UpdateOrderStatusSql,
                               row["BillId"], errorMsg, "106"));
                            return false;
                        }
                    }
                }
            }
            foreach (DataRow taskRow in taskTable.Rows)
            {
                var orderTaskItem = orderTaskTable.AsEnumerable()
                    .FirstOrDefault(item =>
                    item["WarehouseId"].ToString() == taskRow["WarehouseId"].ToString()
                    && item["StorerId"].ToString() == taskRow["StorerId"].ToString()
                    && item["LocationId"].ToString() == taskRow["FromLoc"].ToString()
                    && item["SkuId"].ToString() == taskRow["SkuId"].ToString()
                    && item["CID"].ToString() == taskRow["CID"].ToString()
                    && item["BoxCode"].ToString() == taskRow["BoxCode"].ToString()
                     && item["BatchNo"].ToString() == taskRow["BatchNo"].ToString()
                      && item["SkuStatus"].ToString() == taskRow["SkuStatus"].ToString());
                var task = new TRM_TaskXJDetail
                {
                    OrderId = taskRow["OrderId"].ToString(),
                    OrderLineNo = taskRow["OrderLineNo"].ToString(),
                    StorerId = taskRow["StorerId"].ToString(),
                    WHAreaId = taskRow["WHAreaId"].ToString(),
                    AreaId = taskRow["AreaId"].ToString(),
                    FromLoc = taskRow["FromLoc"].ToString(),
                    ToLoc = taskRow["ToLoc"].ToString(),
                    FromCID = taskRow["CID"].ToString(),
                    BoxCode = taskRow["BoxCode"].ToString(),
                    BatchNo = taskRow["BatchNo"].ToString(),
                    SkuId = taskRow["SkuId"].ToString(),
                    SkuStatus = taskRow["SkuStatus"].ToString(),
                    Qty = taskRow["QtyScan"].ConvertDecimal(),
                    PackageCode = taskRow["PackageCode"].ToString(),
                    PackageQty = taskRow["PackageQty"].ConvertDecimal()
                };
                if (orderTaskItem != null)
                {
                    task.UDF01 = orderTaskItem["UDF01"].ToString();
                    task.UDF02 = orderTaskItem["UDF02"].ToString();
                    task.UDF03 = orderTaskItem["UDF03"].ToString();
                    task.UDF04 = orderTaskItem["UDF04"].ToString();
                    task.UDF05 = orderTaskItem["UDF05"].ToString();
                    task.DUDF01 = orderTaskItem["DUDF01"].ToString();
                    task.DUDF02 = orderTaskItem["DUDF02"].ToString();
                    task.DUDF03 = orderTaskItem["DUDF03"].ToString();
                    task.DUDF04 = orderTaskItem["DUDF04"].ToString();
                    task.DUDF05 = orderTaskItem["DUDF05"].ToString();
                    task.DUDF07 = orderTaskItem["DUDF07"].ToString();
                    task.DUDF08 = orderTaskItem["DUDF08"].ToString();
                    task.DUDF09 = orderTaskItem["DUDF09"].ToString();
                    task.DUDF10 = orderTaskItem["DUDF10"].ToString();
                    task.DUDF11 = orderTaskItem["DUDF11"].ToString();
                }
                tasks.Add(task);
            }
            return true;
        }

        protected void ExecuteOrderAllcate(string billid, T_TRM_TaskXJData data, string updateSql)
        {
            try
            {
                var taskIds = data.TRM_TaskXJ.Select(item => item.TaskId).ToArray();
                var optResult = BusinessDbUtil.InvokeProc(data, updateSql, string.Format(@"EXEC SP_SOM_Order_ALLOCATE '{0}',
'{1}','{2}'", billid, string.Join(",", taskIds), "System"));
                if (optResult.IsSuccess)
                {
                    BusinessDbUtil.ExecuteNonQuery(string.Format(AllocateSQL.UpdateOrderSql, billid, ""));
                    OpUtil.Write("M41", "发货通知单", "AutoAllocate", "自动分配", billid, "SOM_Order", "System");
                }
                else
                {
                    BusinessDbUtil.ExecuteNonQuery(string.Format(AllocateSQL.UpdateOrderSql, billid, optResult.ErrMsg));
                }
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
            }
        }
    }
}
