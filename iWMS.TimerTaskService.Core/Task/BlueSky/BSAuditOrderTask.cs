using Common.DBCore;
using FWCore.TaskManage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FWCore;

namespace iWMS.TimerTaskService.Core.Task
{
    /// <summary>
    /// 蓝天新能源审单程序
    /// </summary>
    public class BSAuditOrderTask : BaseTask
    {
        private const string OrderSql = @"SELECT TOP 100 A.BillId,A.OrderType,A.WarehouseId,A.StorerId,A.WorkMode  
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
                            PickingAllocateHandler handler = new PickingAllocateHandler();
                            handler.Allocate(row);
                        }
                    }
                    //立即执行一次查找库位  和组波
                    if (TaskRunner.Manager != null && TaskRunner.Manager.Tasks != null)
                    {
                        if (TaskRunner.Manager.TaskDict != null)
                        {
                            foreach (var pair in TaskRunner.Manager.TaskDict)
                            {
                                if (pair.Key.TaskId == "ExchangeFindEmptyLocTask" || pair.Key.TaskId == "WavePolicyTask")
                                {
                                    pair.Value.TryExecute(pair.Key);
                                    break;
                                }
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
    }
}
