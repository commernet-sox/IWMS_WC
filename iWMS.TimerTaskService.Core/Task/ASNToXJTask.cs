using Common.DBCore;
using FWCore.TaskManage;
using iWMS.TimerTaskService.Core.Util;
using iWMS.TypedData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FWCore;
using GlobalCache = iWMS.TimerTaskService.Core.Util.GlobalCache;

namespace iWMS.TimerTaskService.Core.Task
{
    /// <summary>
    /// ASN审核后搬运托盘到站台(只考虑 AGV托盘与库位绑定，不允许拿走， POUT042=false)
    /// </summary>
    public class ASNToXJTask : BaseTask
    {
        private const string AllowTakeawayCIDSql = "SELECT ParmValue FROM SYS_Parameter WHERE ParmId='POUT042'";

        private const string SRMOrderSql = @"SELECT COUNT(*) FROM SRM_OrderDetail A WHERE 
EXISTS(SELECT 1 FROM SRM_Order WHERE BillId=A.BillId AND Status='12')";

        protected override bool Execute(TaskConfig config)
        {
            try
            {
                lock (LockObject)
                {
                    var allowTakeawayCID = BusinessDbUtil.ExecuteScalar(AllowTakeawayCIDSql).ConvertString() == "true";
                    if (allowTakeawayCID)
                    {

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
