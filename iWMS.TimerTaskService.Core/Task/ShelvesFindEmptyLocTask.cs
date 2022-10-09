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
    /// 上架找空位
    /// </summary>
    public class ShelvesFindEmptyLocTask : BaseTask
    {
        private const string QueryTaskSql = @"SELECT TOP 1000 *,CAST('' AS NVARCHAR(20)) OrderType INTO #TMP FROM TRM_TaskSJ WHERE ISNULL(ToLoc,'')='' AND Status IN('10')
UPDATE T SET T.OrderType=A.OrderType
FROM #TMP T
INNER JOIN SRM_Order A ON T.SourceBillId=A.BillId
SELECT * FROM #TMP
";
        private const string UpdateTaskSql = "UPDATE TRM_TaskSJ SET IsSuggest=1,ToLoc='{1}',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1,Memo='{2}' WHERE TaskId='{0}'";
        private const string UpdateTaskDetailSql = "UPDATE TRM_TaskSJDetail SET ToLoc='{1}',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1 WHERE TaskId='{0}'";

        private const string InBoundPolicySql = @"SELECT B.* FROM SYS_InBoundPolicy A INNER JOIN SYS_InBoundPolicyDetail B ON A.PolicyId=B.MPolicyId
WHERE A.Status=1 AND B.Status=1 ORDER BY B.Priority DESC";

        private const string OutWHAreaIdSql = @"SELECT DISTINCT WHAreaId+',' FROM(
SELECT  A.WHAreaId FROM TRM_TaskXJ A INNER JOIN SOM_Order B ON A.SourceBillId=B.BillId
WHERE A.SourceBillId='{0}' AND A.CID='{1}' 
UNION ALL
SELECT A.WHAreaId FROM TRM_TaskXJ A INNER JOIN WRM_Inventory B ON A.SourceBillId=B.BillId
WHERE A.SourceBillId='{0}' AND A.CID='{1}') T for xml path('')";

        protected override bool Execute(TaskConfig config)
        {
            try
            {
                lock (LockObject)
                {
                    var policyTable = BusinessDbUtil.GetDataTable(InBoundPolicySql);

                    var table = BusinessDbUtil.GetDataTable(QueryTaskSql);
                    if (table != null && table.Rows.Count > 0)
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            //出库返库优先回原库区  原库区找不到再考虑其它返回方式
                            if (row["TaskType"].ToString()== "MoveIn")
                            {
                                var sourceWHAreaId = BusinessDbUtil.ExecuteScalar(string.Format(OutWHAreaIdSql, row["SourceBillId"],row["CID"])).ConvertString();
                                if (!string.IsNullOrWhiteSpace(sourceWHAreaId))
                                {
                                    var outSuggestTable = BusinessDbUtil.GetDataTable(
                                        string.Format(@"EXEC SP_AGV_Suggest_EmptyLocByWHAreaId '{0}',''", sourceWHAreaId));
                                    if (outSuggestTable != null && outSuggestTable.Rows.Count > 0)
                                    {
                                        var outSuggestLoc = outSuggestTable.Rows[0][0].ConvertString();
                                        if (!string.IsNullOrWhiteSpace(outSuggestLoc))
                                        {
                                            BusinessDbUtil.DoActionWithTrascation(dbUtil =>
                                            {
                                                try
                                                {
                                                    dbUtil.ExecuteNonQuery(string.Format(UpdateTaskSql, row["TaskId"], outSuggestLoc, ""));
                                                    dbUtil.ExecuteNonQuery(string.Format(UpdateTaskDetailSql, row["TaskId"], outSuggestLoc));
                                                }
                                                catch (Exception ex)
                                                {
                                                    NLogUtil.WriteError(ex.ToString());
                                                    throw ex;
                                                }
                                            });
                                            continue;
                                        }
                                    }
                                }
                            }

                            string whareaIds = GetWHAreaIDByPolicy(policyTable, row);

                            var suggestTable = BusinessDbUtil.GetDataTable(string.Format(@"EXEC SP_AGV_SuggestStorage_EmptyLoc '{0}','{1}','{2}','STORAGE','{3}'",
                                row["WarehouseId"], row["StorerId"], whareaIds, row["TaskId"]));
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
                                        dbUtil.ExecuteNonQuery(string.Format(UpdateTaskSql, row["TaskId"], suggestLoc, ""));
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
                                BusinessDbUtil.ExecuteNonQuery(string.Format(UpdateTaskSql, row["TaskId"], "", "找不到推荐库位，请检查是否存在空库位！"));
                            }
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



        private static string GetWHAreaIDByPolicy(DataTable policyTable, DataRow row)
        {
            if (policyTable != null && policyTable.Rows.Count > 0)
            {
                var policyRows = policyTable.AsEnumerable()
                    .Where(item => item["StorerId"].ToString().Contains(row["StorerId"].ToString())
                    && item["OrderTypes"].ToString().Contains(row["OrderType"].ToString()))
                    .ToList();
                if (policyRows.Count > 0)
                {
                    return policyRows[0]["WHAreaIds"].ToString();
                }
            }
            return string.Empty;
        }
    }
}