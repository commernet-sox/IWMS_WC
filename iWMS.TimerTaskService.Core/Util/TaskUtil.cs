using Common.DBCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FWCore;

namespace iWMS.TimerTaskService.Core.Util
{
    public class TaskUtil
    {
        private const string UpdateTaskSql = "UPDATE TRM_TaskXJ SET ToLoc='{1}',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1,Memo='{2}',Prority=ISNULL(Prority,0)+{3} WHERE TaskId='{0}'";
        private const string UpdateTaskDetailSql = "UPDATE TRM_TaskXJDetail SET ToLoc='{1}',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1 WHERE TaskId='{0}'";
        private const string GetLocPutawayorderSql = @"SELECT PutAwayOrder FROM COM_Location WHERE LocationId='{0}'";

        public static void AllocateExchangeLoc(string taskId, string warehouseId, string storerId, string whAreaId, string taskType)
        {
            BusinessDbUtil.DoActionWithTrascation(dbUtil =>
            {
                var suggestTable = dbUtil.GetDataTable(string.Format(@"EXEC SP_AGV_SuggestExchange_EmptyLoc '{0}','{1}','{2}','{3}'", taskId, warehouseId, storerId, whAreaId));
                if (suggestTable != null && suggestTable.Rows.Count > 0)
                {
                    var suggestLoc = suggestTable.Rows[0][0].ConvertString();
                    if (!string.IsNullOrWhiteSpace(suggestLoc))
                    {
                        var putawayOrder = BusinessDbUtil.ExecuteScalar(
                            string.Format(GetLocPutawayorderSql, suggestLoc)).ConvertInt32();
                        var priority = Util.GlobalCache.GetPriorityValue("**OUT" + taskType) + putawayOrder;

                        dbUtil.ExecuteNonQuery(string.Format(UpdateTaskSql, taskId, suggestLoc, "", priority));
                        dbUtil.ExecuteNonQuery(string.Format(UpdateTaskDetailSql, taskId, suggestLoc));
                    }
                }
                else
                {
                    dbUtil.ExecuteNonQuery(string.Format(UpdateTaskSql, taskId, "", "找不到推荐库位，请检查是否存在空库位！"));
                }
            });
        }

        public static void AllocateReplenishmentLoc(string taskId, string whAreaIds, string taskType)
        {
            BusinessDbUtil.DoActionWithTrascation(dbUtil =>
            {
                var suggestTable = dbUtil.GetDataTable(string.Format(@"EXEC SP_AGV_Suggest_EmptyLocByWHAreaId '{0}','{1}'",
                    whAreaIds, taskId));
                if (suggestTable != null && suggestTable.Rows.Count > 0)
                {
                    var suggestLoc = suggestTable.Rows[0][0].ConvertString();
                    if (!string.IsNullOrWhiteSpace(suggestLoc))
                    {
                        var putawayOrder = BusinessDbUtil.ExecuteScalar(
                           string.Format(GetLocPutawayorderSql, suggestLoc)).ConvertInt32();
                        var priority = Util.GlobalCache.GetPriorityValue("**OUT" + taskType) + putawayOrder;

                        dbUtil.ExecuteNonQuery(string.Format(UpdateTaskSql, taskId, suggestLoc, "", priority));
                        dbUtil.ExecuteNonQuery(string.Format(UpdateTaskDetailSql, taskId, suggestLoc));
                    }
                }
                else
                {
                    dbUtil.ExecuteNonQuery(string.Format(UpdateTaskSql, taskId, "", "找不到推荐库位，请检查是否存在空库位！"));
                }
            });
        }
    }
}
