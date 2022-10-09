using Common.DBCore;
using FWCore;
using FWCore.DbComponent;
using FWCore.TaskManage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core.Task
{
    /// <summary>
    /// 点到点搬运任务
    /// </summary>
    public class P2PTask : BaseTask
    {
        private const string P2PTaskSql = @"SELECT TOP 10 * FROM TRM_TaskOther WHERE ISNULL(ToLoc,'')='' AND Status='10' AND TaskType='50'";
        private const string UpdateTaskSql = @"UPDATE TRM_TaskOther SET ToLoc='{1}',ModifyBy='TimerTask',
ModifyDate=GETDATE(),Version=Version+1,Memo='{2}',Priority=ISNULL(Priority,0)+{3} WHERE TaskId='{0}'";
        private const string GetLocPutawayorderSql = @"SELECT PutAwayOrder FROM COM_Location WHERE LocationId='{0}'";

        protected override bool Execute(TaskConfig config)
        {
            try
            {
                lock (LockObject)
                {
                    FindOtherTaskLoc();
                }
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
            }
            return true;
        }

        private static void FindOtherTaskLoc()
        {
            var table = BusinessDbUtil.GetDataTable(P2PTaskSql);
            if (table != null && table.Rows.Count > 0)
            {
                string whAreaIds = string.Empty;
                foreach (DataRow row in table.Rows)
                {
                    switch (row["Ex9"].ToString())
                    {
                        case "EMPTY"://绑空箱
                            {
                                var whAreaTable = BusinessDbUtil.GetDataTable("SELECT WHAreaId FROM COM_WHArea WHERE WHAreaType IN('STORAGE','PICK') ORDER BY AreaOrder ASC");
                                if (whAreaTable != null && whAreaTable.Rows.Count > 0)
                                {
                                    whAreaIds = string.Join(",", whAreaTable.AsEnumerable().Select(item => item["WHAreaId"].ToString()));
                                }
                            }
                            break;
                        case "ASN"://入库空箱到暂存区
                            {
                                whAreaIds = BusinessDbUtil.ExecuteScalar(string.Format(
                                         @"SELECT WHAreaId FROM COM_WorkBench WHERE WbId IN(select WbId from SRM_Order where BillId='{0}')", row["Ex10"])).ConvertString();
                                if (string.IsNullOrWhiteSpace(whAreaIds))
                                {
                                    var whAreaTable = BusinessDbUtil.GetDataTable("SELECT WHAreaId FROM COM_WHArea WHERE WHAreaType IN('TMP_STORAGE') ORDER BY AreaOrder ASC");
                                    if (whAreaTable != null && whAreaTable.Rows.Count > 0)
                                    {
                                        whAreaIds = string.Join(",", whAreaTable.AsEnumerable().Select(item => item["WHAreaId"].ToString()));
                                    }
                                }
                            }
                            break;
                        case "REP"://补货从拣选区空箱到补货站台
                            {
                                var whAreaTable = BusinessDbUtil.GetDataTable(string.Format(
                                    @"SELECT WHAreaId FROM COM_WorkBench WHERE WbId IN (SELECT WbId FROM WRM_Replenishment WHERE BillId='{0}' AND Status<>'00')", row["Ex10"]));
                                if (whAreaTable != null && whAreaTable.Rows.Count > 0)
                                {
                                    whAreaIds = string.Join(",", whAreaTable.AsEnumerable().Select(item => item["WHAreaId"].ToString()));
                                }
                            }
                            break;
                        case "REP_RETURN"://补货从拣选区空箱到补货站台
                            {
                                var whAreaTable = BusinessDbUtil.GetDataTable("SELECT WHAreaId FROM COM_WHArea WHERE WHAreaType IN('STORAGE','PICK') ORDER BY AreaOrder ASC");
                                if (whAreaTable != null && whAreaTable.Rows.Count > 0)
                                {
                                    whAreaIds = string.Join(",", whAreaTable.AsEnumerable().Select(item => item["WHAreaId"].ToString()));
                                }
                            }
                            break;
                        case "SO"://拣货空箱返回原库区
                            {
                                whAreaIds = row["Ex8"].ToString();
                                if (string.IsNullOrWhiteSpace(whAreaIds))
                                {
                                    var whAreaTable = BusinessDbUtil.GetDataTable("SELECT WHAreaId FROM COM_WHArea WHERE WHAreaType IN('STORAGE','PICK') ORDER BY AreaOrder ASC");
                                    if (whAreaTable != null && whAreaTable.Rows.Count > 0)
                                    {
                                        whAreaIds = string.Join(",", whAreaTable.AsEnumerable().Select(item => item["WHAreaId"].ToString()));
                                    }
                                }
                            }
                            break;
                    }
                    var suggestTable = BusinessDbUtil.GetDataTable(string.Format(@"EXEC SP_AGV_Suggest_EmptyLocByWHAreaId '{0}',''",
                        whAreaIds));
                    if (suggestTable != null && suggestTable.Rows.Count > 0)
                    {
                        var suggestLoc = suggestTable.Rows[0][0].ConvertString();
                        if (string.IsNullOrWhiteSpace(suggestLoc))
                        {
                            //BusinessDbUtil.ExecuteNonQuery(string.Format(UpdateTaskSql,
                            //            row["TaskId"], "", "推荐上架库位失败，不存在空库位，请检查！"));
                            continue;
                        }
                        var putawayOrder = BusinessDbUtil.ExecuteScalar(
                            string.Format(GetLocPutawayorderSql, suggestLoc)).ConvertInt32();
                        var priority = Util.GlobalCache.GetPriorityValue("**OTHER" + row["TaskType"]) + putawayOrder;

                        try
                        {
                            BusinessDbUtil.ExecuteNonQuery(string.Format(UpdateTaskSql, row["TaskId"], suggestLoc, "", priority));
                        }
                        catch (Exception ex)
                        {
                            NLogUtil.WriteError(ex.ToString());
                            throw ex;
                        }
                    }
                    else
                    {
                        BusinessDbUtil.ExecuteNonQuery(string.Format(UpdateTaskSql, row["TaskId"], "", "找不到推荐库位，请检查是否存在空库位！", 0));
                    }
                }
            }
        }
    }
}
