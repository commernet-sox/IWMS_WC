using Common.DBCore;
using FWCore;
using FWCore.TaskManage;
using iWMS.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core.Task
{
    public class APPReceiptAutoChargeTask : BaseTask
    {
        private const string OrderSql = "SELECT TOP 100 BillId FROM SRM_Order WHERE Status='45' AND IsCharge=1";
        protected override bool Execute(TaskConfig config)
        {
            try
            {
                DataTable table = BusinessDbUtil.GetDataTable(OrderSql);
                if (table != null && table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        string udf01 = string.Empty;
                        var count = BusinessDbUtil.ExecuteScalar(
                               string.Format("SELECT COUNT(*) FROM SRM_OrderDetail WHERE BillId='{0}' AND ISNULL(UDF01,'')=''", row["BillId"])).ConvertInt32();
                        if (count > 0)
                        {
                            udf01 = "1101";
                        }
                        string procSql = string.Format(@"EXEC SP_SRM_Order_Complete '{0}','{1}','{2}','{3}','{4}','{5}'",
                                  row["BillId"], DateTime.Now, udf01, "TimerTask", IPUtil.GetIP(), Dns.GetHostName());
                        var result = BusinessDbUtil.InvokeProc(procSql);
                        if (!result.IsSuccess)
                        {
                            NLogUtil.WriteError(result.ErrMsg);
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
