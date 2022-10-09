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
    /// 标准订单分配
    /// </summary>
    public class QLSHAllocateHandler : BaseAllocateHandler
    {
        public override bool Allocate(DataRow row)
        {
            var tasks = new List<TRM_TaskXJDetail>();
            var result = CalcOrderTask(tasks, row, false);
            if (!result)
            {
                return false;
            }
            var tuple = CalOrderTaskData(tasks, row);
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
            var taskPriorityItems = new List<TaskPriorityItem>();
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                var groupTask = group.FirstOrDefault();
                var list = group.ToList();

                if (string.IsNullOrWhiteSpace(list[0].ToLoc))
                {
                    BusinessDbUtil.ExecuteNonQuery(string.Format(AllocateSQL.UpdateOrderSql,
                      row["BillId"], "出库目标库位为空！"));
                    return false;
                }

                //主表
                //DataTable toLocTable = null;
                T_TRM_TaskXJData.TRM_TaskXJRow taskRow = data.TRM_TaskXJ.NewTRM_TaskXJRow();
                taskRow.TaskId = taskIds[i];
                var toTable = BusinessDbUtil.GetDataTable(string.Format(
                    @"SELECT B.WHAreaType,A.AisleId,A.PutAwayOrder FROM COM_Location A WITH (NOLOCK) INNER JOIN COM_WHArea B WITH (NOLOCK) ON A.WHAreaId=B.WHAreaId
WHERE A.LocationId='{0}'", list[0].ToLoc));
                string whAreaType = "";
                if (toTable != null && toTable.Rows.Count > 0)
                {
                    whAreaType = toTable.Rows[0]["WHAreaType"].ToString();
                }
                if (whAreaType.ConvertString().ToUpper() == "PRODUCT")
                {
                    taskRow.TaskType = "Out2PL";//出库至产线
                    //toLocTable = GetExchageTable();
                }
                else
                {
                    taskRow.TaskType = "Out2EX";//出库到交换库区
                }

                taskRow.SourceBillId = groupTask.OrderId;
                taskRow.WarehouseId = row["WarehouseId"].ToString();
                taskRow.StorerId = groupTask.StorerId;
                taskRow.AreaId = groupTask.AreaId;
                var locTable = BusinessDbUtil.GetDataTable(string.Format(
                   @"SELECT WHAreaId,AisleId,PickGoodsOrder FROM COM_Location WITH (NOLOCK) WHERE LocationId='{0}'", list[0].FromLoc));
                if (locTable != null && locTable.Rows.Count > 0)
                {
                    taskRow.WHAreaId = locTable.Rows[0]["WHAreaId"].ToString();
                    taskRow.AisleId = locTable.Rows[0]["AisleId"].ToString();

                    if (!string.IsNullOrWhiteSpace(taskRow.AisleId))
                    {
                        taskPriorityItems.Add(new TaskPriorityItem
                        {
                            LocationId = list[0].FromLoc,
                            AisleId = taskRow.AisleId,
                            PickGoodsOrder = locTable.Rows[0]["PickGoodsOrder"].ConvertInt32(),
                            TaskRow = taskRow,
                            CalcCompleted = false
                        });
                    }
                }
                taskRow.Status = "10";
                taskRow.SourceBillType = 1;
                taskRow.CID = list[0].FromCID;
                taskRow.FromLoc = list[0].FromLoc;
                taskRow.ToLoc = list[0].ToLoc;
                //taskRow.ToLoc = GetExchangeLoc(toLocTable, i);
                taskRow.Prority = GlobalCache.GetPriorityValue("**OUT" + taskRow.TaskType);
                taskRow.IsExchange = 0;
                taskRow.IsAgvCompleted = 0;
                taskRow.Version = 0;
                taskRow.CreateBy = "System";
                taskRow.CreateDate = DateTime.Now;
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
                    taskDetailRow.ToLoc = taskRow.ToLoc;
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
                    data.TRM_TaskXJDetail.AddTRM_TaskXJDetailRow(taskDetailRow);
                }
            }
            #region 任务优先级-出库巷道交叉算法
            try
            {
                //重新计算出库交换区优先级
                var priority = 0;
                priority = BusinessDbUtil.ExecuteScalar(string.Format(
                    @"SELECT ISNULL(MAX(Prority),0) FROM TRM_TaskXJ WHERE CreateDate>='{0}'", DateTime.Today.ToString())).ConvertInt32();
                if (priority == 0)
                {
                    if (data.TRM_TaskXJ.Count > 0)
                    {
                        priority = data.TRM_TaskXJ[0].Prority;
                    }
                }
                List<TaskGroupPriority> aisleGroupIds = new List<TaskGroupPriority>();
                var itemGroups = taskPriorityItems.GroupBy(item => item.AisleId).OrderBy(g => g.FirstOrDefault().AisleId);
                foreach (var itemGroup in itemGroups)
                {
                    aisleGroupIds.Add(new TaskGroupPriority { AisleId = itemGroup.FirstOrDefault().AisleId, Group = itemGroup });
                }
                var aisleList = taskPriorityItems.OrderBy(item => item.AisleId + item.PickGoodsOrder).ToList();
                int index = 0;
                for (int i = 0; i < aisleList.Count; i++)
                {
                    if (aisleList[i].CalcCompleted)
                    {
                        continue;
                    }
                    if (aisleList[i].AisleId == aisleGroupIds[index].AisleId)
                    {
                        var taskItem = aisleList[i].TaskRow;
                        taskItem.BeginEdit();
                        taskItem.Prority = priority++;
                        aisleList[i].CalcCompleted = true;
                        //taskItem.Memo = "1";
                        taskItem.EndEdit();

                        if (!aisleGroupIds[index].Group.ToList().Exists(item => !item.CalcCompleted))
                        {
                            aisleGroupIds.Remove(aisleGroupIds[index]);
                            if (index > 0)
                            {
                                index--;
                            }
                        }

                        if (index == aisleGroupIds.Count - 1)
                        {
                            index = 0;
                        }
                        else
                        {
                            index++;
                        }
                    }

                    for (int j = 0; j < aisleList.Count; j++)
                    {
                        if (aisleList[j].CalcCompleted)
                        {
                            continue;
                        }
                        if (aisleList[j].AisleId == aisleGroupIds[index].AisleId)
                        {
                            var taskItem2 = aisleList[j].TaskRow;
                            taskItem2.BeginEdit();
                            taskItem2.Prority = priority++;
                            aisleList[j].CalcCompleted = true;
                            //taskItem2.Memo = "2";
                            taskItem2.EndEdit();

                            if (!aisleGroupIds[index].Group.ToList().Exists(item => !item.CalcCompleted))
                            {
                                aisleGroupIds.Remove(aisleGroupIds[index]);
                                if (index > 0)
                                {
                                    index--;
                                }
                            }

                            if (index == aisleGroupIds.Count - 1)
                            {
                                index = 0;
                            }
                            else
                            {
                                index++;
                            }
                        }
                    }
                }
                //计算不能交叉的巷道，优先级需要排到后面
                for (int i = 0; i < aisleList.Count; i++)
                {
                    if (aisleList[i].CalcCompleted)
                    {
                        continue;
                    }
                    var taskItem = aisleList[i].TaskRow;
                    taskItem.BeginEdit();
                    taskItem.Prority = priority++;
                    aisleList[i].CalcCompleted = true;
                    //taskItem.Memo = "3";
                    taskItem.EndEdit();
                }
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
            }
            #endregion

            ExecuteOrderAllcate(row["BillId"].ToString(), data, tuple.Item2);
            return true;
        }

        private Tuple<bool, StrJoin> CalOrderTaskData(List<TRM_TaskXJDetail> tasks, DataRow row)
        {
            StrJoin sj = string.Empty;
            var detailTable = BusinessDbUtil.GetDataTable(string.Format(@"SELECT C.FlowNo,'{0}',A.StorerId,C.OrderLineNo,C.SkuId,C.BatchNo,C.Qty,C.SkuStatus,C.PackageCode,C.PackageQty,C.UDF01,C.UDF02,C.UDF03,C.UDF04,C.UDF09
FROM SOM_Order A
INNER JOIN SOM_SyncOrder B ON A.BillId=B.BillId
INNER JOIN SOM_OrderDetail C ON A.BillId=C.BillId
WHERE A.BillId='{0}' AND A.Status IN('12')", row["BillId"]));

            foreach (DataRow detailRow in detailTable.Rows)
            {
                if (detailRow["Qty"].ConvertDecimal() <= 0) continue;
                bool enoughQty = false;
                var taskQty = tasks.Where(item =>
                    item.StorerId == detailRow["StorerId"].ToString()
                  && item.SkuId == detailRow["SkuId"].ToString()
                  && item.OrderLineNo == detailRow["OrderLineNo"].ToString()
                  ).Sum(item => item.Qty);
                if (taskQty == detailRow["Qty"].ConvertDecimal())
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
            //if (!string.IsNullOrWhiteSpace(sj))
            //{
            //    sj += string.Format(@"UPDATE SOM_Order SET Status='25',AllocateTaskReason='',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1,AllocatedDate=GETDATE() WHERE BillId='{0}';", row["BillId"]);
            //}
            return new Tuple<bool, StrJoin>(true, sj);
        }
    }

    public class TaskPriorityItem
    {
        public string LocationId { get; set; }
        public string AisleId { get; set; }
        public int PickGoodsOrder { get; set; }
        public T_TRM_TaskXJData.TRM_TaskXJRow TaskRow { get; set; }
        public bool CalcCompleted { get; set; }
    }

    public class TaskGroupPriority
    {
        public string AisleId { get; set; }
        public IGrouping<string, TaskPriorityItem> Group { get; set; }
    }
}
