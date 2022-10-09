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
    /// 库卡美的机械臂叫料
    /// </summary>
    public class MDCallAllocateHandler : BaseAllocateHandler
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
            var groups = tasks.GroupBy(item => item.FromCID).ToList();
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
                taskRow.TaskType = "Out2PL";//出库到产线区
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
                taskRow.CID = list[0].FromCID;
                taskRow.FromLoc = list[0].FromLoc;
                taskRow.ToLoc = list[0].ToLoc;
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
                    taskDetailRow.ToLoc = task.ToLoc;
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
            DataSet dataSet = BusinessDbUtil.GetDataSet(string.Format(AllocateSQL.MD_CALL_OrderINVSql,
                row["BillId"], row["WarehouseId"]));
            var stockTable = dataSet.Tables[0];
            var detailTable = dataSet.Tables[1];
            if ((stockTable == null || stockTable.Rows.Count < 1) && !locAllocate)
            {
                BusinessDbUtil.ExecuteNonQuery(string.Format(AllocateSQL.UpdateOrderStatusSql, row["BillId"], "库存不足", "106"));
                return new Tuple<bool, StrJoin>(false, sj);
            }
            if (detailTable == null || detailTable.Rows.Count < 1)
            {
                return new Tuple<bool, StrJoin>(false, sj);
            }
            var detailRow = detailTable.Rows[0];//叫料当前只考虑明细第一行
            var tmpTasks = new List<TRM_TaskXJDetail>();
            var groups = stockTable.AsEnumerable().GroupBy(item => item["LocationId"].ToString());
            IGrouping<string, DataRow> group = null;
            var skuMinQty = GetSkuMinQty();
            if (skuMinQty > 0)
            {
                foreach (var tmpGroup in groups)
                {
                    var groupList = tmpGroup.ToList();
                    decimal qty = 0;
                    foreach (var tmpRow in groupList)
                    {
                        if (string.IsNullOrWhiteSpace(detailRow["BatchNo"].ToString()))
                        {
                            if (tmpRow["StorerId"].ToString() == detailRow["StorerId"].ToString()
                                && tmpRow["SkuId"].ToString() == detailRow["SkuId"].ToString())
                            {
                                qty += tmpRow["QtyValid"].ConvertDecimal();
                            }
                        }
                        else
                        {
                            if (tmpRow["StorerId"].ToString() == detailRow["StorerId"].ToString()
                                && tmpRow["SkuId"].ToString() == detailRow["SkuId"].ToString() &&
                                tmpRow["BatchNo"].ToString() == detailRow["BatchNo"].ToString())
                            {
                                qty += tmpRow["QtyValid"].ConvertDecimal();
                            }
                        }
                    }
                    if (qty >= skuMinQty)
                    {
                        group = tmpGroup;
                        break;
                    }
                }
            }
            if (group == null)//默认找一托
            {
                foreach (var tmpGroup in groups)
                {
                    var groupList = tmpGroup.ToList();
                    var qty = groupList.Sum(item => item["QtyValid"].ConvertDecimal());
                    if (qty>=skuMinQty)
                    {
                        group = tmpGroup;
                        break;
                    }
                }
            }
            if (group == null)
            {
                BusinessDbUtil.ExecuteNonQuery(string.Format(AllocateSQL.UpdateOrderStatusSql, row["BillId"], "库存不满足条件", "106"));
                return new Tuple<bool, StrJoin>(false, sj);
            }
            else
            {
                sj += string.Format(@"UPDATE SOM_OrderDetail SET Status='30',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1 WHERE BillId='{0}';", row["BillId"]);
            }
            var list = group.ToList();
            var table = BusinessDbUtil.GetDataTable(string.Format(
                @"SELECT LocationId,CID,BoxCode,BatchNo,SkuId,SkuStatus,QtyValid,UDF01,UDF02,UDF03,UDF04,UDF05,DUDF01,DUDF02,DUDF03,DUDF04,DUDF05,DUDF06,DUDF07,DUDF08,DUDF09,
DUDF10,DUDF11 FROM INV_BAL WHERE LocationId='{0}' AND Qty>0", list[0]["LocationId"]));
            foreach (DataRow stockRow in table.Rows)
            {
                tasks.Add(new TRM_TaskXJDetail
                {
                    OrderId = detailRow["OrderId"].ToString(),
                    OrderLineNo = detailRow["OrderLineNo"].ToString(),
                    StorerId = detailRow["StorerId"].ToString(),
                    FromLoc = stockRow["LocationId"].ToString(),
                    ToLoc = detailRow["UDF09"].ToString(),
                    FromCID = stockRow["CID"].ToString(),
                    BoxCode = stockRow["BoxCode"].ToString(),
                    BatchNo = stockRow["BatchNo"].ToString(),
                    SkuId = stockRow["SkuId"].ToString(),
                    SkuStatus = stockRow["SkuStatus"].ToString(),
                    Qty = stockRow["QtyValid"].ConvertDecimal(),
                    PackageCode = detailRow["PackageCode"].ToString(),
                    PackageQty = detailRow["PackageQty"].ConvertDecimal(),
                    UDF01 = stockRow["UDF01"].ToString(),
                    UDF02 = stockRow["UDF02"].ToString(),
                    UDF03 = stockRow["UDF03"].ToString(),
                    UDF04 = stockRow["UDF04"].ToString(),
                    UDF05 = stockRow["UDF05"].ToString(),
                    DUDF01 = stockRow["DUDF01"].ToString(),
                    DUDF02 = stockRow["DUDF02"].ToString(),
                    DUDF03 = stockRow["DUDF03"].ToString(),
                    DUDF04 = stockRow["DUDF04"].ToString(),
                    DUDF05 = stockRow["DUDF05"].ToString(),
                    DUDF06 = stockRow["DUDF06"].ToString(),
                    DUDF07 = stockRow["DUDF07"].ToString(),
                    DUDF08 = stockRow["DUDF08"].ToString(),
                    DUDF09 = stockRow["DUDF09"].ToString(),
                    DUDF10 = stockRow["DUDF10"].ToString(),
                    DUDF11 = stockRow["DUDF11"].ToString()
                });
            }
            return new Tuple<bool, StrJoin>(true, sj);
        }

        private static decimal GetSkuMinQty()
        {
            string sql = @"SELECT ParmValue FROM SYS_Parameter WHERE ParmId='POUT043'";
            var result = BusinessDbUtil.ExecuteScalar(sql);
            return result.ConvertDecimal();
        }
    }
}
