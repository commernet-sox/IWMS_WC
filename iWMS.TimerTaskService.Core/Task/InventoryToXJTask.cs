using Common.DBCore;
using FWCore.TaskManage;
using iWMS.TimerTaskService.Core.Util;
using iWMS.TypedData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FWCore;
using GlobalCache = iWMS.TimerTaskService.Core.Util.GlobalCache;

namespace iWMS.TimerTaskService.Core.Task
{
    /// <summary>
    /// 盘点AGV搬运托盘到交换区
    /// </summary>
    public class InventoryToXJTask : BaseTask
    {
        private const string QuerySql = @"SELECT TOP 10 BillId,WarehouseId,StorerId,(SELECT WHAreaId+',' FROM WRM_InventoryWHArea WHERE BillId=A.BillId for xml path('')) ExchangeWHAreaIds
FROM WRM_Inventory A WHERE A.Status='20' AND A.InventoryMode='AGV' AND EXISTS(SELECT 1 FROM WRM_InventoryStock WHERE BillId=A.BillId AND IsCreateTask=0)";

        private const string StockSql = @"SELECT * FROM WRM_InventoryStock WITH (NOLOCK) WHERE BillId='{0}' AND IsCreateTask=0";

        private const string UpdateStockSql = "UPDATE WRM_InventoryStock SET IsCreateTask=1 WHERE BillId='{0}' AND LocationId='{1}';";

        protected override bool Execute(TaskConfig config)
        {
            try
            {
                lock (LockObject)
                {
                    DataTable table = BusinessDbUtil.GetDataTable(QuerySql);
                    if (table != null && table.Rows.Count > 0)
                    {
                        foreach (DataRow row in table.Rows)
                        {

                            if (string.IsNullOrWhiteSpace(row["ExchangeWHAreaIds"].ToString())) continue;
                            var exchangeTable = BusinessDbUtil.GetDataTable(string.Format("EXEC SP_AGV_SuggestExchange_EmptyLocByWHAreaId '{0}'", row["ExchangeWHAreaIds"]));
                            if (exchangeTable == null || exchangeTable.Rows.Count < 1)
                            {
                                continue;
                            }

                            var stockTable = BusinessDbUtil.GetDataTable(string.Format(StockSql, row["BillId"]));
                            if (stockTable == null || stockTable.Rows.Count < 1)
                            {
                                continue;
                            }
                            var groups = stockTable.AsEnumerable().GroupBy(item => item["LocationId"].ToString()).ToList();
                            int count = groups.Count > exchangeTable.Rows.Count ? exchangeTable.Rows.Count : groups.Count;
                            string[] taskIds = BillIdGenUtil.GenTaskId(row["WarehouseId"].ToString(), row["StorerId"].ToString(), count);
                            if (taskIds == null || taskIds.Length < 1)
                            {
                                NLogUtil.WriteError(row["BillId"] + "取单号失败，请联系管理员！");
                                continue;
                            }
                            T_TRM_TaskXJData data = new T_TRM_TaskXJData();
                            StrJoin updateSql = "";
                            for (int i = 0; i < count; i++)
                            {
                                var group = groups[i];
                                var groupTask = group.FirstOrDefault();
                                var list = group.ToList();

                                //主表
                                T_TRM_TaskXJData.TRM_TaskXJRow taskRow = data.TRM_TaskXJ.NewTRM_TaskXJRow();
                                taskRow.TaskId = taskIds[i];
                                taskRow.TaskType = "Stock2EX";//盘点到交换库区
                                taskRow.SourceBillId = groupTask["BillId"].ToString();
                                taskRow.WarehouseId = row["WarehouseId"].ToString();
                                taskRow.StorerId = groupTask["StorerId"].ToString();
                                taskRow.AreaId = string.Empty;
                                var locTable = BusinessDbUtil.GetDataTable(string.Format(
                                    @"SELECT WHAreaId,AisleId FROM COM_Location WITH (NOLOCK) WHERE LocationId='{0}'", groupTask["LocationId"]));
                                if (locTable != null && locTable.Rows.Count > 0)
                                {
                                    taskRow.WHAreaId = locTable.Rows[0]["WHAreaId"].ToString();
                                    taskRow.AisleId = locTable.Rows[0]["AisleId"].ToString();
                                }
                                taskRow.Status = "10";
                                taskRow.SourceBillType = 1;
                                taskRow.CID = groupTask["CID"].ToString();
                                taskRow.FromLoc = groupTask["LocationId"].ToString();
                                taskRow.ToLoc = exchangeTable.Rows[i][0].ToString();
                                taskRow.Prority = GlobalCache.GetPriorityValue("**Inventory" + taskRow.TaskType);
                                taskRow.IsExchange = 0;
                                taskRow.IsAgvCompleted = 0;
                                taskRow.Version = 0;
                                taskRow.CreateBy = "System";
                                taskRow.CreateDate = DateTime.Now;
                                taskRow.UDF01 = groupTask["UDF01"].ToString();
                                taskRow.UDF02 = groupTask["UDF02"].ToString();
                                taskRow.UDF03 = groupTask["UDF03"].ToString();
                                taskRow.UDF04 = groupTask["UDF04"].ToString();
                                taskRow.UDF05 = groupTask["UDF05"].ToString();
                                data.TRM_TaskXJ.AddTRM_TaskXJRow(taskRow);

                                //明细表
                                foreach (var task in list)
                                {
                                    T_TRM_TaskXJData.TRM_TaskXJDetailRow taskDetailRow = data.TRM_TaskXJDetail.NewTRM_TaskXJDetailRow();
                                    taskDetailRow.TaskId = taskIds[i];
                                    taskDetailRow.OrderId = task["BillId"].ToString();
                                    taskDetailRow.OrderLineNo = task["FlowNo"].ToString();
                                    taskDetailRow.FromCID = task["CID"].ToString();
                                    taskDetailRow.ToCID = task["CID"].ToString();
                                    taskDetailRow.FromLoc = task["LocationId"].ToString();
                                    taskDetailRow.ToLoc = exchangeTable.Rows[i][0].ToString();
                                    taskDetailRow.BatchNo = task["BatchNo"].ToString();
                                    taskDetailRow.FromBoxCode = task["BoxCode"].ToString();
                                    taskDetailRow.ToBoxCode = task["BoxCode"].ToString();
                                    taskDetailRow.SkuId = task["SkuId"].ToString();
                                    taskDetailRow.SkuStatus = task["SkuStatus"].ToString();
                                    taskDetailRow.QtyPlan = task["Qty"].ConvertDecimal(); ;
                                    taskDetailRow.QtyScan = 0;
                                    taskDetailRow.PackageCode = task["PackageCode"].ToString();
                                    taskDetailRow.PackageQty = task["PackageQty"].ConvertDecimal();
                                    taskDetailRow.Version = 0;
                                    taskDetailRow.CreateBy = "System";
                                    taskDetailRow.CreateDate = DateTime.Now;
                                    taskDetailRow.UDF01 = task["DUDF01"].ToString();
                                    taskDetailRow.UDF02 = task["DUDF02"].ToString();
                                    taskDetailRow.UDF03 = task["DUDF03"].ToString();
                                    taskDetailRow.UDF04 = task["DUDF04"].ToString();
                                    taskDetailRow.UDF05 = task["DUDF05"].ToString();
                                    taskDetailRow.UDF06 = task["DUDF06"].ToString();
                                    taskDetailRow.UDF07 = task["DUDF07"].ToString();
                                    taskDetailRow.UDF08 = task["DUDF08"].ToString();
                                    taskDetailRow.UDF09 = task["DUDF09"].ToString();
                                    taskDetailRow.UDF10 = task["DUDF10"].ToString();
                                    taskDetailRow.UDF11 = task["DUDF11"].ToString();
                                    data.TRM_TaskXJDetail.AddTRM_TaskXJDetailRow(taskDetailRow);
                                }


                                updateSql += string.Format(UpdateStockSql, row["BillId"], groupTask["LocationId"]);
                            }
                            BusinessDbUtil.Save(data, updateSql);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
            }
            return true;
        }


    }
}
