using Common.DBCore;
using FWCore.DbComponent;
using FWCore.TaskManage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FWCore;
using iWMS.TimerTaskService.Core.Util;

namespace iWMS.TimerTaskService.Core.Task
{
    /// <summary>
    /// 下架到交换区查找库位
    /// </summary>
    public class ExchangeFindEmptyLocTask : BaseTask
    {
        private const string QueryTaskSql = "SELECT TOP 1000 * FROM TRM_TaskXJ WHERE ISNULL(ToLoc,'')='' AND Status IN('10') AND TaskType IN('MoveOut','Out2EX','MoveSample')";

        private const string QueryReplenishmentSql = @"SELECT TOP 1000 * FROM TRM_TaskXJ WHERE ISNULL(ToLoc,'')='' AND Status IN('10') AND TaskType IN('Rep2EX')";

        protected override bool Execute(TaskConfig config)
        {
            try
            {
                lock (LockObject)
                {
                    FindExchangeEmptyLoc();

                    FindReplenishmentEmptyLoc();
                }
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
            }
            return true;
        }

        private void FindExchangeEmptyLoc()
        {
            var table = BusinessDbUtil.GetDataTable(QueryTaskSql);
            if (table != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        TaskUtil.AllocateExchangeLoc(row["TaskId"].ToString(),
                            row["WarehouseId"].ToString(),
                            row["StorerId"].ToString(),
                            row["WHAreaId"].ToString(),
                            row["TaskType"].ToString());
                    }
                    catch (Exception ex)
                    {
                        NLogUtil.WriteError(ex.ToString());
                        throw ex;
                    }
                }
            }
        }

        private void FindReplenishmentEmptyLoc()
        {
            var table = BusinessDbUtil.GetDataTable(QueryReplenishmentSql);
            if (table != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        string whAreaIds = string.Empty;
                        string wbId = row["WbId"].ToString();
                        if (!string.IsNullOrWhiteSpace(wbId))
                        {
                            whAreaIds = BusinessDbUtil.ExecuteScalar(string.Format(
                                @"SELECT WHAreaId FROM COM_WorkBench WHERE WbId='{0}'", wbId)).ConvertString();
                        }
                        if (string.IsNullOrWhiteSpace(whAreaIds) || string.IsNullOrWhiteSpace(wbId))
                        {
                            var whAreaTable = BusinessDbUtil.GetDataTable("SELECT WHAreaId FROM COM_WHArea WHERE WHAreaType='TMP_STORAGE'");
                            if (whAreaTable!=null && whAreaTable.Rows.Count>0)
                            {
                                whAreaIds = string.Join(",", whAreaTable.AsEnumerable().Select(item => item["WHAreaId"].ToString()));
                            }
                        }

                        TaskUtil.AllocateReplenishmentLoc(row["TaskId"].ToString(), whAreaIds, row["TaskType"].ToString());
                    }
                    catch (Exception ex)
                    {
                        NLogUtil.WriteError(ex.ToString());
                        throw ex;
                    }
                }
            }
        }
    }
}
