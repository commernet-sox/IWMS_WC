using Common.DBCore;
using FWCore.TaskManage;
using System;
using System.Data;

namespace iWMS.TimerTaskService.Core.Task
{
    /// <summary>
    /// 潍柴二号工厂审单程序
    /// </summary>
    public class WC2AuditOrderTask : BaseTask
    {
        private const string OrderSql = @"SELECT TOP 100 A.BillId,A.OrderType,A.WarehouseId,A.StorerId,A.WorkMode,B.Priority   
FROM SOM_Order A WITH(READPAST) INNER JOIN SOM_SyncOrder B WITH(READPAST) ON A.BillId=B.BillId WHERE A.Status IN('10','12','25','35','40') AND A.AutoAllocateComplete=0 AND EXISTS(SELECT 1 FROM SOM_OrderDetail WHERE BillId=A.BillId AND Status='12')";

        protected override bool Execute(TaskConfig config)
        {
            try
            {
                lock (LockObject)
                {
                    DataTable table = BusinessDbUtil.GetDataTable(OrderSql);
                    if (table != null && table.Rows.Count > 0)
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            WC2AllocateHandler handler = new WC2AllocateHandler();
                            handler.Allocate(row);
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
