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
    /// 国网永康审单程序
    /// </summary>
    public class GWYKAuditOrderTask : BaseTask
    {
//        private const string UpdateLackOrderSql = @"DECLARE @OrderId NVARCHAR(30)
//SELECT TOP 1 @OrderId=BillId FROM SOM_Order WHERE Status='106' AND OrderType='NORMAL'
//IF ISNULL(@OrderId,'')<>''
//BEGIN
//UPDATE SOM_Order SET Status='12',ModifyBy='TimerTask',ModifyDate=GETDATE(),
//Version=Version+1,AutoAllocateComplete=0,AllocateTaskReason='' WHERE BillId=@OrderId AND Status='106'

//UPDATE SOM_OrderDetail SET Status='12',ModifyBy='TimerTask',ModifyDate=GETDATE(),
//Version=Version+1 WHERE BillId=@OrderId AND Status='106'
//END";

        private const string OrderSql = @"SELECT TOP 100 A.BillId,A.OrderType,A.WarehouseId,A.StorerId,A.WorkMode  
FROM SOM_Order A WITH(READPAST) INNER JOIN SOM_SyncOrder B WITH(READPAST) ON A.BillId=B.BillId WHERE A.Status IN('10','12','25','35','40') AND A.AutoAllocateComplete=0 AND EXISTS(SELECT 1 FROM SOM_OrderDetail WHERE BillId=A.BillId AND Status='12')";

        //private const string POUT041Sql = "SELECT ParmValue FROM SYS_Parameter WHERE ParmId='POUT041'";
        private const string ExistsUNCompleteOrderSql = @"SELECT 1 FROM SOM_Order WHERE BillId<>'{0}' AND Status>='25' AND Status<'55'";

        protected override bool Execute(TaskConfig config)
        {
            try
            {
                lock (LockObject)
                {
                    //BusinessDbUtil.GetDataTable(UpdateLackOrderSql);

                    DataTable table = BusinessDbUtil.GetDataTable(OrderSql);
                    if (table != null && table.Rows.Count > 0)
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            //if (row["OrderType"].ToString() == "NORMAL")
                            //{
                            //    var POUT041 = BusinessDbUtil.ExecuteScalar(POUT041Sql).ConvertString();
                            //    if (POUT041 == "true")
                            //    {
                            //        var result = BusinessDbUtil.ExecuteScalar(string.Format(ExistsUNCompleteOrderSql, row["BillId"]));
                            //        if (result.ConvertInt32() > 0)
                            //        {
                            //            continue;
                            //        }
                            //    }
                            //}
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
