using Common.DBCore;
using iWMS.WCInterfaceService.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core.Util
{
    public class BillIdGenUtil
    {
        public static string[] GenTaskId(string warehouseId, string storerId, int count)
        {
            try
            {
                string sql = string.Format("EXEC SP_GEN_TASKID_BATCH 'TXJ','{0}','{1}',{2}", storerId, warehouseId, count);
                DataTable table = BusinessDbUtil.GetDataTable(sql);
                if (table != null && table.Rows.Count > 0)
                {
                    return table.AsEnumerable().Select(item => item[0].ToString()).ToArray();
                }
                return null;
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(string.Format(@"取单号异常：{0}\r\nStorer:{1},Warehouse:{2}", ex.ToString(), storerId, warehouseId));
            }
            return null;
        }

        public static string GenBillId(string prefix, string warehouseId, string storerId, string type)
        {
            try
            {
                string sql = string.Format("DECLARE @Id nvarchar(30) EXEC SP_GEN_COMMON_ID '{0}','{1}','{2}','{3}',@Id OUTPUT SELECT @Id", prefix, storerId, warehouseId, type);
                DataTable table = BusinessDbUtil.GetDataTable(sql);
                if (table != null && table.Rows.Count > 0)
                {
                    return table.Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(string.Format(@"取单号异常：{0}\r\nStorer:{1},Warehouse:{2}", ex.ToString(), storerId, warehouseId));
            }
            return null;
        }
    }
}
