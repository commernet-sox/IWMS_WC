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
    /// 叫空托和返回空托任务（金红叶）
    /// </summary>
    public class APPEmptyPalletTask : BaseTask
    {
        private const string EmptyPalletTaskSql = "SELECT TOP 1000 * FROM TRM_TaskOther WHERE ISNULL(FromLoc,'')='' AND Status='10' AND TaskType='20'";
        private const string EmptyPalletBackTaskSql = "SELECT TOP 1000 * FROM TRM_TaskOther WHERE ISNULL(ToLoc,'')='' AND Status='10' AND TaskType='60'";
        private const string EmptyPalletGroupTaskSql= "SELECT TOP 1000 * FROM TRM_TaskOther WHERE ISNULL(FromLoc,'')='' AND Status='10' AND TaskType='98'";
        private const string EmptyPalletGroupBackTaskSql = "SELECT TOP 1000 * FROM TRM_TaskOther WHERE ISNULL(ToLoc,'')='' AND Status='10' AND TaskType='99'";
        private const string UpdateTaskSql = "UPDATE TRM_TaskOther SET {3}='{1}',CID='{4}',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1,Memo='{2}' WHERE TaskId='{0}'";

        protected override bool Execute(TaskConfig config)
        {
            try
            {
                lock (LockObject)
                {
                    DoEmptyPallet();
                    DoEmptyPalletBack();
                    DoEmptyPalletGroup();
                    DoEmptyPalletGroupBack();
                }
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
            }
            return true;
        }

        private void DoEmptyPallet()
        {
            var table = BusinessDbUtil.GetDataTable(EmptyPalletTaskSql);
            if (table != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    if (!string.IsNullOrWhiteSpace(row["WorkshopId"].ToString()) || !string.IsNullOrWhiteSpace(row["MachineLineId"].ToString()))
                    {
                        ExecuteSql(string.Format(@"EXEC SP_COM_Location_Update '{0}','TimerTask','load'", row["WHAreaId"]), row["TaskId"].ToString(), "FromLoc");
                    }
                    else
                    {
                        ExecuteSql(string.Format(@"EXEC SP_COM_Location_Update '{0}','TimerTask','load',1", row["WHAreaId"]), row["TaskId"].ToString(), "FromLoc");
                    }
                }
            }
        }

        private void DoEmptyPalletBack()
        {
            var table = BusinessDbUtil.GetDataTable(EmptyPalletBackTaskSql);
            if (table != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    if (!string.IsNullOrWhiteSpace(row["WorkshopId"].ToString()) || !string.IsNullOrWhiteSpace(row["MachineLineId"].ToString()))
                    {
                        ExecuteSql(string.Format(@"EXEC SP_COM_Location_Update '{0}','TimerTask','unload'", row["WHAreaId"]), row["TaskId"].ToString(), "ToLoc");
                    }
                    else
                    {
                        ExecuteSql(string.Format(@"EXEC SP_COM_Location_Update '{0}','TimerTask','unload',1", row["WHAreaId"]), row["TaskId"].ToString(), "ToLoc");
                    }
                }
            }
        }

        private void DoEmptyPalletGroup()
        {
            var table = BusinessDbUtil.GetDataTable(EmptyPalletGroupTaskSql);
            if (table != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    if (!string.IsNullOrWhiteSpace(row["WorkshopId"].ToString()) || !string.IsNullOrWhiteSpace(row["MachineLineId"].ToString()))
                    {
                        ExecuteSql(string.Format(@"EXEC SP_COM_Location_Update '{0}','TimerTask','stackload'", row["WHAreaId"]), row["TaskId"].ToString(), "FromLoc");
                    }
                    else
                    {
                        ExecuteSql(string.Format(@"EXEC SP_COM_Location_Update '{0}','TimerTask','stackload',1", row["WHAreaId"]), row["TaskId"].ToString(), "FromLoc");
                    }
                }
            }
        }


        private void DoEmptyPalletGroupBack()
        {
            var table = BusinessDbUtil.GetDataTable(EmptyPalletGroupBackTaskSql);
            if (table != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    if (!string.IsNullOrWhiteSpace(row["WorkshopId"].ToString()) || !string.IsNullOrWhiteSpace(row["MachineLineId"].ToString()))
                    {
                        ExecuteSql(string.Format(@"EXEC SP_COM_Location_Update '{0}','TimerTask','stackunload'", row["WHAreaId"]), row["TaskId"].ToString(), "ToLoc");
                    }
                    else
                    {
                        ExecuteSql(string.Format(@"EXEC SP_COM_Location_Update '{0}','TimerTask','stackunload',1", row["WHAreaId"]), row["TaskId"].ToString(), "ToLoc");
                    }
                }
            }
        }

        private void ExecuteSql(string procSql,string taskId,string locationField)
        {
            using (IDbUtil dbUtil = DBManager.Build(DBUtil.DBType, DBUtil.BussinessConnectString))
            {
                try
                {
                    dbUtil.BeginTrans();

                    if (!string.IsNullOrWhiteSpace(procSql))
                    {
                        var table = dbUtil.GetDataTable(procSql);
                        if (table != null && table.Rows.Count > 0)
                        {
                            DataRow row = table.Rows[0];
                            var issuccess = row["issuccess"].ConvertString();
                            var locationId = row["LocationName"].ConvertString();
                            var cid = row["CID"].ConvertString();
                            var errmsg = row["errmsg"].ConvertString();
                            if (issuccess != "1")
                            {
                                BusinessDbUtil.ExecuteNonQuery(string.Format(UpdateTaskSql, taskId, "", errmsg, locationField, ""));
                                dbUtil.Commit();
                                return;
                            }

                            var effectCount = dbUtil.ExecuteNonQuery(
                                string.Format(UpdateTaskSql, taskId, locationId, "", locationField, cid));
                            if (effectCount < 1)
                            {
                                dbUtil.RollBack();
                            }
                            else
                            {
                                dbUtil.Commit();
                            }
                        }
                        else
                        {
                            NLogUtil.WriteError(DateTime.Now + "执行存储过程出错！");
                        }
                    }
                }
                catch (Exception ex)
                {
                    dbUtil.RollBack();
                    NLogUtil.WriteError(ex.ToString());
                }
            }
        }
    }
}
