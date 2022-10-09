using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.DBCore;
using FWCore;
using FWCore.DbComponent;
using FWCore.TaskManage;
using iWMS.Core;

namespace iWMS.TimerTaskService.Core.Task
{
    /// <summary>
    /// 金红叶上架找空位（根据配置物料上架库区，优先考虑单向货架）
    /// </summary>
    public class APPShelvesFindEmptyLocTask : BaseTask
    {
        private const string QueryTaskSql = "SELECT TOP 1000 * FROM TRM_TaskSJ WHERE ISNULL(ToLoc,'')='' AND Status IN('10')";
        private const string UpdateTaskSql = "UPDATE TRM_TaskSJ SET IsSuggest=1,ToLoc='{1}',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1,Memo='{2}',Prority=ISNULL(Prority,0)+{3} WHERE TaskId='{0}'";
        private const string UpdateTaskDetailSql = "UPDATE TRM_TaskSJDetail SET ToLoc='{1}',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1 WHERE TaskId='{0}'";
        private const string GetLocPutawayorderSql = @"SELECT PutAwayOrder FROM COM_Location WHERE LocationId='{0}'";

        protected override bool Execute(TaskConfig config)
        {
            try
            {
                var table = BusinessDbUtil.GetDataTable(QueryTaskSql);
                if (table != null && table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var suggestTable = BusinessDbUtil.GetDataTable(string.Format(@"EXEC SP_AGV_SuggestStorage_AllEmptyLoc '{0}','{1}','{2}','{3}'", 
                            row["TaskId"], row["WarehouseId"], row["StorerId"], row["WHAreaId"]));
                        if (suggestTable != null && suggestTable.Rows.Count > 0)
                        {
                            var suggestLoc = suggestTable.Rows[0][0].ConvertString();
                            if (string.IsNullOrWhiteSpace(suggestLoc))
                            {
                                //BusinessDbUtil.ExecuteNonQuery(string.Format(UpdateTaskSql,
                                //            row["TaskId"], "", "推荐上架库位失败，不存在空库位，请检查！"));
                                continue;
                            }
                            BusinessDbUtil.DoActionWithTrascation(dbUtil =>
                            {
                                try
                                {
                                    var putawayOrder = BusinessDbUtil.ExecuteScalar(
                          string.Format(GetLocPutawayorderSql, suggestLoc)).ConvertInt32();

                                    dbUtil.ExecuteNonQuery(string.Format(UpdateTaskSql, row["TaskId"], suggestLoc, "", putawayOrder));
                                    dbUtil.ExecuteNonQuery(string.Format(UpdateTaskDetailSql, row["TaskId"], suggestLoc));
                                }
                                catch (Exception ex)
                                {
                                    NLogUtil.WriteError(ex.ToString());
                                    throw ex;
                                }
                            });
                        }
                        else
                        {
                            BusinessDbUtil.ExecuteNonQuery(string.Format(UpdateTaskSql, row["TaskId"], "", "找不到推荐库位，请检查是否存在空库位！",0));
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
