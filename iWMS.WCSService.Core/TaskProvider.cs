using Common.DBCore;
using FWCore.TaskManage;
using iWMS.APILog;
using System;
using System.Collections.Generic;
using System.Data;

namespace iWMS.WCSService.Core
{
    public class TaskProvider
    {
        public static IList<TaskConfig> GetTaskConfigList()
        {
            try
            {
                List<TaskConfig> tasks = new List<TaskConfig>();
                string sql = "SELECT * FROM SYS_TimerTask WHERE TaskType='Interface' AND Status='1'";
                var table = BusinessDbUtil.GetDataTable(sql);
                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        tasks.Add(new TaskConfig
                        {
                            TaskId = row["TaskId"].ToString(),
                            TaskName = row["TaskName"].ToString(),
                            Group = row["TaskType"].ToString(),
                            DllPath = row["FileName"].ToString(),
                            ClassPath = row["OpenName"].ToString(),
                            CronExpression = row["CronExpression"].ToString(),
                            Row = row
                        });
                    }
                }
                return tasks;
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
                throw ex;
            }
        }
    }
}
