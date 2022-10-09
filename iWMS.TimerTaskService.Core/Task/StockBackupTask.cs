using Common.DBCore;
using FWCore;
using FWCore.TaskManage;
using iWMS.TimerTaskService.Core.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core.Task
{
    /// <summary>
    /// 库存备份
    /// </summary>
    public class StockBackupTask : BaseTask
    {
        private const string INV_BAL_SQL = @"SELECT * FROM INV_BAL WITH (NOLOCK) WHERE Qty>0";

        protected override bool Execute(TaskConfig config)
        {
            try
            {
                lock (LockObject)
                {
                    var stockDir = ConfigurationUtil.GetAppSettingString("StockDir");
                    if (string.IsNullOrWhiteSpace(stockDir))
                    {
                        NLogUtil.WriteError("缺少配置项StockDir");
                        return false;
                    }
                    string dirPath = Path.Combine(stockDir, DateTime.Now.ToString("yyyyMMdd"));
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }
                    CheckBackUpData(stockDir);
                    string filePath = Path.Combine(dirPath, "Stock" + DateTime.Now.ToString("yyyyMMdd") + ".XLSX");
                    var table = BusinessDbUtil.GetDataTable(INV_BAL_SQL);
                    table.TableName = "INV_BAL";
                    if (table != null)
                    {
                        var dict = new Dictionary<string, string>();
                        foreach (DataColumn column in table.Columns)
                        {
                            if (!dict.ContainsKey(column.ColumnName))
                            {
                                dict.Add(column.ColumnName, column.ColumnName);
                            }
                        }
                        ExcelHelper.ExportToExcel(filePath, dict, table);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
            }
            return true;
        }

        private void CheckBackUpData(string dirPath)
        {
            try
            {
                int keeyDays = ConfigUtil.StockBackupKeepDays;
                var dirs = new DirectoryInfo(dirPath).GetDirectories("*", SearchOption.TopDirectoryOnly);
                var minValue = dirs.Min(item => int.Parse(item.Name));
                var minDate = DateTime.Parse(minValue.ToString().Substring(0, 4) + "-" + minValue.ToString().Substring(4, 2) + "-" + minValue.ToString().Substring(6, 2));
                TimeSpan ts = DateTime.Today - minDate;
                if (ts.Days >= keeyDays)
                {
                    var keeyDate = DateTime.Today.AddDays(-keeyDays);
                    foreach (var dir in dirs)
                    {
                        if (int.Parse(dir.Name) <= int.Parse(keeyDate.ToString("yyyyMMdd")))
                        {
                            var files = dir.GetFiles("*.XLSX", SearchOption.AllDirectories);
                            foreach (var file in files)
                            {
                                file.Delete();
                            }
                            if (dir.GetFiles("*", SearchOption.AllDirectories).Length < 1)
                            {
                                dir.Delete();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
            }
        }


    }
}
