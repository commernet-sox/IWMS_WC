using Common.DBCore;
using FWCore;
using iWMS.Entity;
using iWMS.TimerTaskService.Core.Util;
using iWMS.TypedData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalCache = iWMS.TimerTaskService.Core.Util.GlobalCache;

namespace iWMS.TimerTaskService.Core.Task
{
    /// <summary>
    /// MES叫料
    /// </summary>
    public class SDXSAllocateHandler : BaseAllocateHandler
    {
        public override bool Allocate(DataRow row)
        {
            var tasks = new List<TRM_TaskXJDetail>();
            var tuple = Calc(tasks, row);
            if (!tuple.Item1)
            {
                return false;
            }
            T_TRM_TaskXJData data = new T_TRM_TaskXJData();
            var groups = tasks.GroupBy(item => item.FromLoc).ToList();
            int count = groups.Count;
            if (count < 1)
            {
                BusinessDbUtil.ExecuteNonQuery(string.Format(AllocateSQL.UpdateOrderStatusSql, row["BillId"], "库存不足", "106"));
                return false;
            }

            string[] taskIds = BillIdGenUtil.GenTaskId(row["WarehouseId"].ToString(), row["StorerId"].ToString(), count);
            if (taskIds == null || taskIds.Length < 1)
            {
                BusinessDbUtil.ExecuteNonQuery(string.Format(AllocateSQL.UpdateOrderSql,
                      row["BillId"], "取单号失败，请联系管理员！"));
                return false;
            }
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                var groupTask = group.FirstOrDefault();
                var list = group.ToList();

                //主表
                T_TRM_TaskXJData.TRM_TaskXJRow taskRow = data.TRM_TaskXJ.NewTRM_TaskXJRow();
                taskRow.TaskId = taskIds[i];
                taskRow.SourceBillId = groupTask.OrderId;
                taskRow.WarehouseId = row["WarehouseId"].ToString();
                taskRow.StorerId = groupTask.StorerId;
                taskRow.AreaId = groupTask.AreaId;
                taskRow.Status = "10";
                taskRow.SourceBillType = 1;
                var locTable = BusinessDbUtil.GetDataTable(string.Format(
                    @"SELECT WHAreaId,AisleId FROM COM_Location WITH (NOLOCK) WHERE LocationId='{0}'", list[0].FromLoc));
                if (locTable != null && locTable.Rows.Count > 0)
                {
                    taskRow.WHAreaId = locTable.Rows[0]["WHAreaId"].ToString();
                    taskRow.AisleId = locTable.Rows[0]["AisleId"].ToString();
                }
                taskRow.TaskType = taskRow.WHAreaId == "C3" ? "Out2PL" : "Out2EX";
                taskRow.CID = list[0].FromCID;
                taskRow.FromLoc = list[0].FromLoc;
                if (taskRow.TaskType == "Out2PL")
                {
                    taskRow.ToLoc = list[0].ToLoc;
                }
                else
                {
                    taskRow.ToLoc = string.Empty;
                }

                taskRow.Prority = GlobalCache.GetPriorityValue("**OUT" + taskRow.TaskType);
                taskRow.IsExchange = 0;
                taskRow.IsAgvCompleted = 0;
                taskRow.Version = 0;
                taskRow.CreateBy = "System";
                taskRow.CreateDate = DateTime.Now;
                taskRow.UDF01 = groupTask.UDF01;
                taskRow.UDF02 = groupTask.UDF02;
                taskRow.UDF03 = groupTask.UDF03;
                taskRow.UDF04 = groupTask.UDF04;
                taskRow.UDF05 = groupTask.UDF05;
                data.TRM_TaskXJ.AddTRM_TaskXJRow(taskRow);

                //明细表
                foreach (var task in list)
                {
                    T_TRM_TaskXJData.TRM_TaskXJDetailRow taskDetailRow = data.TRM_TaskXJDetail.NewTRM_TaskXJDetailRow();
                    taskDetailRow.Status = "10";
                    taskDetailRow.TaskId = taskIds[i];
                    taskDetailRow.OrderId = task.OrderId;
                    taskDetailRow.OrderLineNo = task.OrderLineNo;
                    taskDetailRow.FromCID = task.FromCID;
                    taskDetailRow.ToCID = task.ToCID;
                    taskDetailRow.FromLoc = task.FromLoc;
                    taskDetailRow.ToLoc = groups.Count == 1 ? task.ToLoc : "";
                    taskDetailRow.BatchNo = task.BatchNo;
                    taskDetailRow.FromBoxCode = task.BoxCode;
                    taskDetailRow.ToBoxCode = task.BoxCode;
                    taskDetailRow.SkuId = task.SkuId;
                    taskDetailRow.SkuStatus = task.SkuStatus;
                    taskDetailRow.QtyPlan = task.Qty;
                    taskDetailRow.QtyScan = 0;
                    taskDetailRow.PackageCode = task.PackageCode;
                    taskDetailRow.PackageQty = task.PackageQty;
                    taskDetailRow.Version = 0;
                    taskDetailRow.CreateBy = "System";
                    taskDetailRow.CreateDate = DateTime.Now;
                    taskDetailRow.UDF01 = task.DUDF01;
                    taskDetailRow.UDF02 = task.DUDF02;
                    taskDetailRow.UDF03 = task.DUDF03;
                    taskDetailRow.UDF04 = task.DUDF04;
                    taskDetailRow.UDF05 = task.DUDF05;
                    taskDetailRow.UDF06 = task.DUDF06;
                    taskDetailRow.UDF07 = task.DUDF07;
                    taskDetailRow.UDF08 = task.DUDF08;
                    taskDetailRow.UDF09 = task.DUDF09;
                    taskDetailRow.UDF10 = task.DUDF10;
                    taskDetailRow.UDF11 = task.DUDF11;
                    data.TRM_TaskXJDetail.AddTRM_TaskXJDetailRow(taskDetailRow);
                }
            }

            ExecuteOrderAllcate(row["BillId"].ToString(), data, tuple.Item2);

            return true;
        }

        private Tuple<bool, StrJoin> Calc(List<TRM_TaskXJDetail> tasks, DataRow row)
        {
            StrJoin sj = string.Empty;
            bool locAllocate = tasks.Count > 0;
            DataSet dataSet = BusinessDbUtil.GetDataSet(string.Format(AllocateSQL.SDXS_CALL_OrderINVSql,
                row["BillId"], row["WarehouseId"]));
            var stockTable = dataSet.Tables[0];
            var detailTable = dataSet.Tables[1];
            if (locAllocate)
            {
                foreach (DataRow detailRow in detailTable.Rows)
                {
                    bool enoughQty = false;
                    var taskQty = tasks.Where(item =>
                    item.StorerId == detailRow["StorerId"].ToString()
                      && item.SkuId == detailRow["SkuId"].ToString()
                      && item.OrderLineNo == detailRow["OrderLineNo"].ToString()).Sum(item => item.Qty);
                    if (detailRow["QtyPlan"].ConvertDecimal() == 0 || taskQty == detailRow["QtyPlan"].ConvertDecimal())
                    {
                        enoughQty = true;
                    }
                    else
                    {
                        enoughQty = false;
                    }

                    if (!enoughQty)
                    {
                        sj += string.Format(@"UPDATE SOM_OrderDetail SET Status='25',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1 WHERE FlowNo={0};", detailRow["FlowNo"]);
                    }
                    else
                    {
                        sj += string.Format(@"UPDATE SOM_OrderDetail SET Status='30',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1 WHERE FlowNo={0};", detailRow["FlowNo"]);
                    }
                }
            }
            else
            {
                if (stockTable != null && stockTable.Rows.Count < 1)
                {
                    foreach (DataRow detailRow in detailTable.Rows)
                    {
                        BusinessDbUtil.ExecuteNonQuery(string.Format(AllocateSQL.UpdateOrderDetailStatusSql, detailRow["OrderId"], detailRow["FlowNo"], "106"));
                    }
                    BusinessDbUtil.ExecuteNonQuery(string.Format(AllocateSQL.UpdateOrderStatusSql, row["BillId"], "库存不足", "106"));
                    return new Tuple<bool, StrJoin>(false, sj);
                }
            }
            if (detailTable == null || detailTable.Rows.Count < 1)
            {
                return new Tuple<bool, StrJoin>(false, sj);
            }
            foreach (DataRow detailRow in detailTable.Rows)
            {
                if (detailRow["Qty"].ConvertDecimal() <= 0) continue;

                var tmpTasks = new List<TRM_TaskXJDetail>();
                bool enoughQty = false;
                foreach (DataRow stockRow in stockTable.Rows)
                {
                    if (stockRow["QtyValid"].ConvertDecimal() <= 0) continue;
                    if (ExistsStockData(detailRow, stockRow))
                    {
                        #region 存在库存扣减
                        if (detailRow["Qty"].ConvertDecimal() <= stockRow["QtyValid"].ConvertDecimal())
                        {
                            if (stockRow["WHAreaId"].ToString() == "C3")
                            {
                                var taskQtyCount = tmpTasks.Sum(item => item.Qty);
                                tasks.Add(new TRM_TaskXJDetail
                                {
                                    OrderId = detailRow["OrderId"].ToString(),
                                    OrderLineNo = detailRow["OrderLineNo"].ToString(),
                                    StorerId = detailRow["StorerId"].ToString(),
                                    FromLoc = stockRow["LocationId"].ToString(),
                                    ToLoc = detailRow["DUDF09"].ToString(),
                                    FromCID = stockRow["CID"].ToString(),
                                    BoxCode = stockRow["BoxCode"].ToString(),
                                    BatchNo = stockRow["BatchNo"].ToString(),
                                    SkuId = stockRow["SkuId"].ToString(),
                                    SkuStatus = stockRow["SkuStatus"].ToString(),
                                    Qty = stockRow["QtyValid"].ConvertDecimal(),
                                    PackageCode = detailRow["PackageCode"].ToString(),
                                    PackageQty = detailRow["PackageQty"].ConvertDecimal(),
                                    UDF01 = stockRow["UDF01"].ToString()
                                });

                                stockRow.BeginEdit();
                                stockRow["QtyValid"] = 0;
                                stockRow.EndEdit();
                                enoughQty = true;
                                break;
                            }
                            else
                            {
                                tasks.AddRange(tmpTasks.ToArray());
                                var taskQtyCount = tmpTasks.Sum(item => item.Qty);
                                tasks.Add(new TRM_TaskXJDetail
                                {
                                    OrderId = detailRow["OrderId"].ToString(),
                                    OrderLineNo = detailRow["OrderLineNo"].ToString(),
                                    StorerId = detailRow["StorerId"].ToString(),
                                    FromLoc = stockRow["LocationId"].ToString(),
                                    ToLoc = detailRow["DUDF09"].ToString(),
                                    FromCID = stockRow["CID"].ToString(),
                                    BoxCode = stockRow["BoxCode"].ToString(),
                                    BatchNo = stockRow["BatchNo"].ToString(),
                                    SkuId = stockRow["SkuId"].ToString(),
                                    SkuStatus = stockRow["SkuStatus"].ToString(),
                                    Qty = detailRow["Qty"].ConvertDecimal() - taskQtyCount,
                                    PackageCode = detailRow["PackageCode"].ToString(),
                                    PackageQty = detailRow["PackageQty"].ConvertDecimal(),
                                    UDF01 = stockRow["UDF01"].ToString()
                                });

                                stockRow.BeginEdit();
                                stockRow["QtyValid"] = stockRow["QtyValid"].ConvertDecimal() - detailRow["Qty"].ConvertDecimal() + taskQtyCount;
                                stockRow.EndEdit();
                                enoughQty = true;
                                break;
                            }
                        }
                        else
                        {
                            if (stockRow["WHAreaId"].ToString() == "C3")
                            {
                                continue;
                            }
                            var taskQtyCount = tmpTasks.Sum(item => item.Qty);
                            if (taskQtyCount + stockRow["QtyValid"].ConvertDecimal() < detailRow["Qty"].ConvertDecimal())
                            {
                                tmpTasks.Add(new TRM_TaskXJDetail
                                {
                                    OrderId = detailRow["OrderId"].ToString(),
                                    OrderLineNo = detailRow["OrderLineNo"].ToString(),
                                    StorerId = detailRow["StorerId"].ToString(),
                                    FromLoc = stockRow["LocationId"].ToString(),
                                    ToLoc = detailRow["DUDF09"].ToString(),
                                    FromCID = stockRow["CID"].ToString(),
                                    BoxCode = stockRow["BoxCode"].ToString(),
                                    BatchNo = stockRow["BatchNo"].ToString(),
                                    SkuId = stockRow["SkuId"].ToString(),
                                    SkuStatus = stockRow["SkuStatus"].ToString(),
                                    Qty = stockRow["QtyValid"].ConvertDecimal(),
                                    PackageCode = detailRow["PackageCode"].ToString(),
                                    PackageQty = detailRow["PackageQty"].ConvertDecimal(),
                                    UDF01 = stockRow["UDF01"].ToString()
                                });

                                stockRow["QtyValid"] = 0;
                            }
                            else
                            {
                                tmpTasks.Add(new TRM_TaskXJDetail
                                {
                                    OrderId = detailRow["OrderId"].ToString(),
                                    OrderLineNo = detailRow["OrderLineNo"].ToString(),
                                    StorerId = detailRow["StorerId"].ToString(),
                                    FromLoc = stockRow["LocationId"].ToString(),
                                    ToLoc = detailRow["DUDF09"].ToString(),
                                    FromCID = stockRow["CID"].ToString(),
                                    BoxCode = stockRow["BoxCode"].ToString(),
                                    BatchNo = stockRow["BatchNo"].ToString(),
                                    SkuId = stockRow["SkuId"].ToString(),
                                    SkuStatus = stockRow["SkuStatus"].ToString(),
                                    Qty = detailRow["Qty"].ConvertDecimal() - taskQtyCount,
                                    PackageCode = detailRow["PackageCode"].ToString(),
                                    PackageQty = detailRow["PackageQty"].ConvertDecimal(),
                                    UDF01 = stockRow["UDF01"].ToString()
                                });

                                stockRow["QtyValid"] = stockRow["QtyValid"].ConvertDecimal() - (detailRow["Qty"].ConvertDecimal() - taskQtyCount);

                                tasks.AddRange(tmpTasks.ToArray());
                                enoughQty = true;
                                break;
                            }
                        }
                        #endregion
                    }
                }
                if (!enoughQty)
                {
                    tasks.AddRange(tmpTasks.ToArray());

                    var stockQtyCount = tmpTasks.Sum(item => item.Qty);
                    if (stockQtyCount <= 0)
                    {
                        sj += string.Format(@"UPDATE SOM_OrderDetail SET Status='106',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1  WHERE FlowNo={0};", detailRow["FlowNo"]);
                    }
                    else
                    {
                        if (stockQtyCount < detailRow["Qty"].ConvertDecimal())
                        {
                            sj += string.Format(@"UPDATE SOM_OrderDetail SET Status='25',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1  WHERE FlowNo={0};", detailRow["FlowNo"]);
                        }
                    }
                }
                else
                {
                    sj += string.Format(@"UPDATE SOM_OrderDetail SET Status='30',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1  WHERE FlowNo={0};", detailRow["FlowNo"]);
                }
            }
            //if (!string.IsNullOrWhiteSpace(sj))
            //{
            //    sj += string.Format(@"UPDATE SOM_Order SET Status='25',AllocateTaskReason='',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1,AllocatedDate=GETDATE() WHERE BillId='{0}';", row["BillId"]);
            //}
            return new Tuple<bool, StrJoin>(true, sj);
        }

        private bool ExistsStockData(DataRow detailRow, DataRow stockRow)
        {
            if (!string.IsNullOrWhiteSpace(detailRow["BatchNo"].ToString()))
            {
                return detailRow["StorerId"].ToString().Equals(stockRow["StorerId"].ToString(), StringComparison.OrdinalIgnoreCase)
                        && detailRow["SkuId"].ToString().Equals(stockRow["SkuId"].ToString(), StringComparison.OrdinalIgnoreCase)
                      && detailRow["BatchNo"].ToString().Equals(stockRow["BatchNo"].ToString(), StringComparison.OrdinalIgnoreCase)
                      && detailRow["UDF01"].ToString().Equals(stockRow["UDF01"].ToString(), StringComparison.OrdinalIgnoreCase);
            }
            return detailRow["StorerId"].ToString().Equals(stockRow["StorerId"].ToString(), StringComparison.OrdinalIgnoreCase)
                        && detailRow["SkuId"].ToString().Equals(stockRow["SkuId"].ToString(), StringComparison.OrdinalIgnoreCase)
                        && detailRow["UDF01"].ToString().Equals(stockRow["UDF01"].ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
